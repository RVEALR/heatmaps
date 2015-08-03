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

import sys, datetime, dateutil.parser, getopt, json
from urllib import urlretrieve
from urllib2 import Request, urlopen, URLError, HTTPError

version_num = '0.0.1'
short_args = "o:f:l:srctudvh"
long_args = [ "output=,first=","last=",
              "start","running","custom","transaction","user","device",
              "version","help"
            ]

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


def usage():
  msg = """usage: get_raw_events.py \'<link-to-data-export>\'
  \t[-f|--first <date>] [-l|--last <date>]
  \t[-s|--start] [-r|--running][-c|--custom][-t|--transaction][-u|--user][-d|--device]
  \t[-v|--version] [-h|--help]\n
  version\t\tRetrieve version info for this file.
  help\t\tPrint this help message.
  first\t\tUNIX timestamp for trimming input.
  last\t\tUNIX timestamp for trimming input.
  start\t\tFlag. Include appStart events.
  running\t\tFlag. Include appRunning events.
  custom\t\tFlag. Include custom events
  transaction\tFlag. Include transaction events.
  user\t\tFlag. Include userInfo events.
  device\t\tFlag. Include deviceInfo events"""
  print msg

def version_info():
  print 'get_raw_events.py Raw data export by Unity Analytics. (c)2015 Version: ' + version_num

def main(argv):
  output_path = ''
  start_date = ''
  end_date = ''
  include_all = True
  include_start = False
  include_running = False
  include_custom = False
  include_transaction = False
  include_user = False
  include_device = False

  try:
    opts, args = getopt.getopt(argv[1:], short_args, long_args)
    url = '' if len(sys.argv) < 2 else sys.argv[1]
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
    elif opt in ("-o", "--output"):
      output_path = arg
    elif opt in ("-f", "--first"):
      start_date = dateutil.parser.parse(arg)
    elif opt in ("-l", "--last"):
      end_date = dateutil.parser.parse(arg)
    elif opt in ("-s", "--start"):
      include_all = False
      include_start = True
    elif opt in ("-r", "--running"):
      include_all = False
      include_running = True
    elif opt in ("-c", "--custom"):
      include_all = False
      include_custom = True
    elif opt in ("-t", "--transaction"):
      include_all = False
      include_transaction = True
    elif opt in ("-u", "--user"):
      include_all = False
      include_user = True
    elif opt in ("-d", "--device"):
      include_all = False
      include_device = True

  # if first arg was help, instead of a url
  if url == '-h' or url == '--help':
    usage()
    sys.exit(2)

  if len(url) > 0:

    print 'Loading batch manifest'

    if end_date == '':
      end_date = datetime.datetime.utcnow()
    if start_date == '':
      start_date = end_date - datetime.timedelta(days=5)  # subtract 5 days by default

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
        batch_type = "" # FIXME
        
        # by default, we d/l everything, but if flags are set, we trim unmatched types
        batch_type = ''
        if bUrl.find('appStart') > -1:
          batch_type = 'appStart'
          if include_all == False and include_start == False:
            continue
        elif bUrl.find('custom') > -1:
          batch_type = 'custom'
          if include_all == False and include_custom == False:
            continue
        elif bUrl.find('deviceInfo') > -1:
          batch_type = 'deviceInfo'
          if include_all == False and include_device == False:
            continue
        elif bUrl.find('appRunning') > -1:
          batch_type = 'appRunning'
          if include_all == False and include_running == False:
            continue
        elif bUrl.find('transaction') > -1:
          batch_type = 'transaction'
          if include_all == False and include_transaction == False:
            continue
        elif bUrl.find('userInfo') > -1:
          batch_type = 'userInfo'
          if include_all == False and include_user == False:
            continue

        # finally, load the actual file from S3
        output_file_name = output_path + batch_id + "_" + batch_type + ".txt"
        try:
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
    usage()
    sys.exit(2)

if __name__ == "__main__":
   main(sys.argv[1:])