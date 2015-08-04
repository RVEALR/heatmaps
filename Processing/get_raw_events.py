#!/usr/bin/python

# written for and tested with Python 2.7.6

#Examples (NOTE THAT URL MUST BE IN QUOTES):
#    BASIC: retrieves last five days, all event types
#    get_raw_events.py 'https://analytics.cloud.unity3d.com/api/v1/batches?appid=SOME_AP_ID&hash=SOME_HASH'
#    TRIM FIRST AND LAST DATES
#    get_raw_events.py 'https://analytics.cloud.unity3d.com/api/v1/batches?appid=SOME_AP_ID&hash=SOME_HASH' -f '2016-01-03' -l '2016-01-05'
#    LIMIT WHICH BATCHES TO DOWNLOAD (THIS EXAMPLE ONLY DOWNLOADS CUSTOM EVENTS (-c) AND DEVICE INFO (-d))
#    get_raw_events.py 'https://analytics.cloud.unity3d.com/api/v1/batches?appid=SOME_AP_ID&hash=SOME_HASH' -c -d
#    OUTPUT TO A DIFFERENT DIRECTORY
#    get_raw_events.py 'https://analytics.cloud.unity3d.com/api/v1/batches?appid=SOME_AP_ID&hash=SOME_HASH' -o ../my_data/

import sys, datetime, dateutil.parser, argparse, json
from urllib import urlretrieve
from urllib2 import Request, urlopen, URLError, HTTPError

version_num = '0.0.1'
all_events = ['appStart','appRunning','custom','transaction','userInfo','deviceInfo']

def load_file(url):
  req = Request(url)
  try:
    response = urlopen(req)
  except HTTPError as e:
    print 'The server couldn\'t fulfill the request.'
    print 'Error code: ', e.code
    sys.exit()
  except URLError as e:
    print 'We failed to reach a server.'
    print 'Reason: ', e.reason
    sys.exit()
  else:
    displayUrl = url if len(url) < 150 else url[:150] + '...'
    print 'Load successful.\n' + url
    return response

def load_and_parse(url):
  response = load_file(url)
  json = parse_json(response)
  print 'JSON successfully parsed'
  return json

def parse_json(response):
  try:
    j = response.read()
    return json.loads(j)
  except ValueError:
    print 'Decoding JSON has failed'
    sys.exit()

def version_info():
  print 'get_raw_events.py Raw data export by Unity Analytics. (c)2015 Version: ' + version_num

def main(argv):
  parser = argparse.ArgumentParser(description="Download raw events from the Unity Analytics server.")
  parser.add_argument('url', nargs='?', default='')
  parser.add_argument('-v', '--version', action='store_const', const=True, help='Retrieve version info for this file.')
  parser.add_argument('-o', '--output', default='', help='Set an output path for results.')
  parser.add_argument('-f', '--first', help='UNIX timestamp for trimming input.')
  parser.add_argument('-l', '--last', help='UNIX timestamp for trimming input.')
  parser.add_argument('-s', '--appStart', action='store_const', const=True, help='Include appStart events.')
  parser.add_argument('-r', '--appRunning', action='store_const', const=True, help='Include appRunning events.')
  parser.add_argument('-c', '--custom', action='store_const', const=True, help='Include custom events.')
  parser.add_argument('-t', '--transaction', action='store_const', const=True, help='Include transaction events.')
  parser.add_argument('-u', '--userInfo', action='store_const', const=True, help='Include userInfo events.')
  parser.add_argument('-d', '--deviceInfo', action='store_const', const=True, help='Include deviceInfo events.')
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
    # subtract 5 days by default
    start_date = end_date - datetime.timedelta(days=5) if not args['first'] else dateutil.parser.parse(args['first'])
  except:
    print 'Provided start date could not be parsed. Format should be YYYY-MM-DD.'
    sys.exit()

  url = args['url']
  
  # by default, we'll include all. If a flag(s) was selected, use it
  flags = []
  for e in all_events:
    if args[e]: flags.append(e)
  if len(flags) == 0:
    flags = all_events

  # if first arg isn't a url
  if 'http' not in url:
    parser.print_help()
    sys.exit(2)
  elif len(url) > 0:
    print 'Loading batch manifest'
    manifest_json = load_and_parse(url)
    
    found_items = 0
    for manifest_item in manifest_json:
      # filter dates outside of range
      date = dateutil.parser.parse(manifest_item["generated_at"]).replace(tzinfo=None)
      if date < start_date:
        continue
      elif date > end_date:
        continue

      found_items += 1
      batches_json = load_and_parse(manifest_item["url"])
      batch_id = batches_json["batchid"]
      for batch in batches_json["data"]:
        bUrl = batch["url"]
        for event_type in flags:
          if event_type in bUrl:
            output_file_name = args['output'] + batch_id + "_" + event_type + ".txt"
            try:
              # finally, load the actual file from S3
              print 'Downloading ' + output_file_name
              urlretrieve(bUrl, output_file_name)
            except HTTPError as e:
              print 'The server couldn\'t download the file.'
              print 'Error code: ', e.code
              sys.exit()
            except URLError as e:
              print 'When downloading, we failed to reach a server.'
              print 'Reason: ', e.reason
              sys.exit()
            else:
              print 'TSV file downloaded successfully'

    if found_items == 0:
      print 'No data found within specified dates. By default, this script downloads the last five days of data. Use -f (--first) and -l (--last) to specify a date range.'
  else:
    print 'get_raw_events.py requires that you specify a URL as the first argument.\nThis URL may be obtained by going to your project settings on the Unity Analytics website.\n\n'
    parser.print_help()
    sys.exit(2)

if __name__ == "__main__":
   main(sys.argv[1:])