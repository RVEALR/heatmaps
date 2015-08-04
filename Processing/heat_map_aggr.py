#!/usr/bin/python

#Examples: 
#    BASIC
#    heat_map_aggr.py -i my_data.tsv
#    CONCATENATE SEVERAL INPUT FILES INTO A SINGLE OUTPUT (USES FIRST FILE TO NAME OUTPUT)
#    heat_map_aggr.py -i 'my_data.tsv,my_other_data.tsv'
#    SPECIFY OUTPUT FILE NAME
#    heat_map_aggr.py -i my_data.tsv -o zaphod.json
#    SMOOTH space (s) AND time (t)
#    heat_map_aggr.py -i my_data.tsv -o my_output.json -s 100 -t 1
#    TRIM first AND last
#    heat_map_aggr.py -i my_data.tsv --first "2015-11-01" --last "2016-02-15"
#    TRIM TO A SPECIFIC SET OF EVENT NAMES
#    heat_map_aggr.py -i my_data.tsv --event-names ["PlayerDeath", "BotKill"]
#    AGGREGATE INDIVIDUAL PLAY SESSIONS
#    heat_map_aggr.py -i my_data.tsv -n
#    DISAGGREGATE EVENTS BY TIME
#    heat_map_aggr.py -i my_data.tsv -d


# Input data is a TSV with entries like so:
# 2015-06-24 17:05:28	test_69	HeatMapPlayerDeath	{"x":"10.0000024","y":"-24.0034234232","z":"5.277900660936861","t":"97.06499906516409","unity.name":"HeatMapPlayerDeath"}
# [0] Unix timestamp
# [1] SessionID
# [2] EventName
# [3] Coordinates (x/y/z/t)


import sys, argparse, csv, dateutil.parser, datetime, json, os

version_num = '0.0.1'

def divide(value, divisor):
  v = float(value)
  d = float(divisor)
  mod = v % d
  rounded = (round(v/d) * d)
  if mod > d/2:
    rounded -= d/2
  else:
    rounded += d/2
  return rounded

def version_info():
  print 'heat_map_aggr.py Heat Map Aggregator by Unity Analytics. (c)2015 Version: ' + version_num

def create_key(point_map, tupl, point):
  point_map[tupl] = point

def get_existing_point(point_map, tupl):
  return point_map[tupl] if tupl in point_map else None

def main(argv):
  parser = argparse.ArgumentParser(description="Aggregate raw event data into JSON that can be read by the Unity Analytics heat map system.")
  parser.add_argument('input', nargs='?', default='', help='The name of an input file, or an array of input files (required).')
  parser.add_argument('-v', '--version', action='store_const', const=True, help='Retrieve version info for this file.')
  parser.add_argument('-o', '--output', help='The name of the output file. If omitted, name is auto-generated from first input file.')
  parser.add_argument('-s', '--space', default=0, help='Numerical scale at which to smooth out spatial data.')
  parser.add_argument('-t', '--time', default=0, help='Numerical scale at which to smooth out temporal data.')
  parser.add_argument('-f', '--first', help='UNIX timestamp for trimming input. 365 days before last by default.')
  parser.add_argument('-l', '--last', help='UNIX timestamp for trimming input. Now by default.')
  parser.add_argument('-e', '--event-names', help='A string or array of strings, indicating event names to include in the output.')
  parser.add_argument('-n', '--single-session', action='store_const', const=True, help='Organize the data by individual play sessions.')
  parser.add_argument('-d', '--disaggregate-time', action='store_const', const=True, help='Disaggregates events that map to matching x/y/z coordinates, but different moments in time.')
  parser.add_argument('-u', '--userInfo', action='store_const', const=True, help='Include userInfo events.')
  args = vars(parser.parse_args())
  
  if 'help' in args:
    parser.print_help()
    sys.exit()
  elif args['version'] == True:
    version_info()
    sys.exit()
  
  try:
    # now by default
    end_date = datetime.datetime.utcnow() if not args['last'] else dateutil.parser.parse(args['last'])
  except:
    print 'Provided end date could not be parsed. Format should be YYYY-MM-DD.'
    sys.exit()

  try:
    # allow 'forever' in start_date unspecified
    start_date = datetime.datetime(2000, 1, 1) if not args['first'] else dateutil.parser.parse(args['first'])
  except:
    print 'Provided start date could not be parsed. Format should be YYYY-MM-DD.'
    sys.exit()

  space_divisor = args['space']
  time_divisor = args['time']
  input_files = args['input'].split(',')
  event_names = args['event_names'].split(',') if args['event_names'] else []

  if len(input_files) == 0:
    print 'heat_map_aggr.py requires that you specify an input file. It\'s not really that much to ask.'
    parser.print_help()
    sys.exit(2)
  else:
    output_data = {}
    point_map = {}
    output_file_name = args['output'] if args['output'] else os.path.splitext(input_files[0])[0] + '.json'
    #loop and smooth all file data
    for fname in input_files:
        with open(fname) as input_file:
          tsv = csv.reader(input_file, delimiter='\t')
          for row in tsv:
            # ignore blank rows
            if len(row) >= 3:
              #ignore rows outside any date trimming
              row_date = dateutil.parser.parse(row[0])
              if row_date <= start_date:
                continue
              if row_date >= end_date:
                continue

              point = {}
              datum = json.loads(row[3])
              event = str(datum['unity.name'])

              # if we're filtering events, pass if not in list
              if len(event_names) > 0 and event not in event_names:
                continue
              # ensure we have a list for this event
              if not event in output_data:
                output_data[event] = []

              # Deal with spatial data
              # x/y are required
              try:
                x = float(datum['x'])
                y = float(datum['y'])
              except KeyError:
                print 'An event in this data set can\'t be interpreted as heat map data. Perhaps you need to filter events using -e?'
                print datum
                sys.exit(2)

              # z values are optional (Vector2's use only x/y)
              try:
                z = float(datum['z'])
              except KeyError:
                z = 0

              if space_divisor > 0:
                x = divide(x, space_divisor)
                y = divide(y, space_divisor)
                z = divide(z, space_divisor)
              point['x'] = x
              point['y'] = y
              point['z'] = z

              # Deal with temporal data, which is also optional
              try:
                t = float(datum['t']) if 't' in datum else 1.0
                if time_divisor > 0:
                  t = divide(t, time_divisor)
                point['t'] = t
              except AttributeError:
                # We allow for the possibility of the dev not including 't' for time.
                # This is faster than hasattr(datum, 't')
                t = 0

              # Hash the point, so we can aggregate for density
              timeKey = point["t"] if args['disaggregate_time'] else None
              sessionKey = datum[1] if args['single_session'] else None
              tupl = (event, point["x"], point["y"], point["z"], timeKey, sessionKey)

              pt = get_existing_point(point_map, tupl)

              if pt == None:
                create_key(point_map, tupl, point)
                point['d'] = 1
                output_data[event].append(point)
              else:
                pt['d'] += 1
    # test if any data was generated
    has_data = False
    report = []
    for generated in output_data:
      try:
        has_data = len(output_data[generated]) > 0
        report.append(len(output_data[generated]))
      except KeyError:
        pass

    if has_data:
      print 'Processed ' + str(len(report)) + ' group(s) with the following numbers of data points: ' + str(report)
      text_file = open(output_file_name, "w")
      text_file.write(json.dumps(output_data))
      zz = text_file.close()
    else:
      print 'The process yielded no results. Could you have misspelled the event name?'

if __name__ == "__main__":
   main(sys.argv[1:])
