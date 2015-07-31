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


import sys, csv, getopt, json, os, dateutil.parser

version_num = '0.0.1'
short_args = "i:o:s:t:f:l:e:ndvh"
long_args = [ "input=","output=",
              "space=","time=",
              "first=","last=",
              "event-names=",
              "single-session",
              "disaggregate-time",
              "version","help"
            ]

def usage():
  msg = 'usage: heat_map_aggr.py -i|--input <input_files_or_array> [-o|--output <output_file_name>]\n'
  msg += '\t[-s|--space <float>] [-t|--time <float>]\n'
  msg += '\t[-f|--first <date>] [-l|--last <date>]\n'
  msg += '\t[-e|--event-names <eventNameList>] [-n|--single-session]\n'
  msg += '\t[-d|disaggregate-time] [-v|--version] [-h|--help]\n\n'
  msg += 'version\t\t\tRetrieve version info for this file.\n'
  msg += 'help\t\t\tPrint this help message.\n'
  msg += 'input\t\t\tThe name of an input file, or an array of input files (required).\n'
  msg += 'output\t\t\tThe name of the output file. If omitted, name is auto-generated from first input file.\n'
  msg += 'space\t\t\tNumerical scale at which to smooth out spatial data.\n'
  msg += 'time\t\t\tNumerical scale at which to smooth out temporal data.\n'
  msg += 'first\t\t\tUNIX timestamp for trimming input.\n'
  msg += 'last\t\t\tUNIX timestamp for trimming input.\n'
  msg += 'event-names\t\tA string or array of strings, indicating event names to include in the output.\n'
  msg += 'single-session\t\tFlag. Organize the data by individual play sessions.\n'
  msg += 'disaggregate-time\tDisaggregates events that map to matching x/y/z coordinates, but different moments in time.\n'
  print msg
  #'\n\t\n\t'

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
  output_data = {}
  point_map = {}
  
  input_files = []
  output_file_name = ''
  space_smooth = False
  space_divisor = 1.0
  time_smooth = False
  time_divisor = 1.0
  start_date = ''
  end_date = ''
  events_list = []
  disaggregate_time = False
  single_session = False

  try:
    opts, args = getopt.getopt(argv, short_args, long_args)
  except getopt.GetoptError:
    usage()
    sys.exit(2)

  for opt, arg in opts:
    if opt in ("-h", "--help"):
      usage()
      sys.exit()
    if opt in ("-v", "--version"):
      version_info()
      sys.exit()
    elif opt in ("-i", "--input"):
      input_files = arg.split(',')
    elif opt in ("-o", "--output"):
      output_file_name = arg
    elif opt in ("-s", "--space"):
      space_smooth = True
      space_divisor = arg
    elif opt in ("-t", "--time"):
      time_smooth = True
      time_divisor = arg
    elif opt in ("-f", "--first"):
      start_date = arg
    elif opt in ("-l", "--last"):
      end_date = arg
    elif opt in ("-e", "--event-names"):
      events_list = arg.split(',')
    elif opt in ("-d", "--disaggregate-time"):
      disaggregate_time = True
    elif opt in ("-n", "--single-session"):
      single_session = True

  if input_files:
    if output_file_name == '':
      # Unless specified, output file gets name matching input
      output_file_name = os.path.splitext(input_files[0])[0] + '.json'
    
    #loop and smooth all file data
    for fname in input_files:
        with open(fname) as input_file:
          tsv = csv.reader(input_file, delimiter='\t')
          for row in tsv:
            # ignore blank rows
            if len(row) >= 3:
              #ignore rows outside any date trimming
              if start_date != '' and row[0] < start_date:
                continue
              elif end_date != '' and row[0] > end_date:
                continue
              point = {}
              datum = json.loads(row[3])
              event = str(datum['unity.name'])
              # if we're filtering events, pass if not in list
              if len(events_list) > 0 and event not in events_list:
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

              if space_smooth:
                x = divide(x, space_divisor)
                y = divide(y, space_divisor)
                z = divide(z, space_divisor)
              point['x'] = x
              point['y'] = y
              point['z'] = z

              # Deal with temporal data, which is also optional
              try:
                t = float(datum['t']) if 't' in datum else 1.0
                if time_smooth:
                  t = divide(t, time_divisor)
                point['t'] = t
              except AttributeError:
                # We allow for the possibility of the dev not including 't' for time.
                # This is faster than hasattr(datum, 't')
                t = 0

              # Hash the point, so we can aggregate for density
              timeKey = point["t"] if disaggregate_time else None
              sessionKey = datum[1] if single_session else None
              tupl = (event, point["x"], point["y"], point["z"], timeKey, sessionKey)

              pt = get_existing_point(point_map, tupl)
              if pt == None:
                create_key(point_map, tupl, point)
                point['d'] = 1
                output_data[event].append(point)
              else:
                pt['d'] += 1

    text_file = open(output_file_name, "w")
    text_file.write(json.dumps(output_data))
    zz = text_file.close()
  else:
    print 'heat_map_aggr.py requires that you specify an input file. It\'s not really that much to ask.'
    usage()
    sys.exit(2)

if __name__ == "__main__":
   main(sys.argv[1:])
