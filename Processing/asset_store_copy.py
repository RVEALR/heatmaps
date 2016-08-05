#!/usr/bin/python

#Examples: 
#    Makes a copy of this repo, excluding the bits not included in the Asset Store release


import sys, argparse, os, shutil

version_num = '0.0.1'

include_these = ['Heatmaps']
then_exclude_these_directories = ['Heatmaps/obj','Heatmaps/RawData','Heatmaps/Assets/Scenes','Heatmaps/Assets/Scripts','Heatmaps/Assets/Plugins/Heatmaps/Examples']
then_exclude_these_files = ['Heatmaps/Assets/HeatmapGliderTarget.cs','Heatmaps/Assets/HeatmapGliderTarget.cs.meta','Heatmaps/Assets/Plugins/Heatmaps/Examples.meta','Heatmaps/Assets/Plugins/Heatmaps/Heatmaps.mdproj','Heatmaps/Assets/Plugins/Heatmaps/Heatmaps.mdproj.meta']


def version_info():
  print 'asset_store_copy.py Asset Store Copy Utility by Unity Analytics. (c)2016 Version: ' + version_num

def main(argv):
  parser = argparse.ArgumentParser(description="Make a copy of this repo, excluding the bits not included in the Asset Store release.")
  parser.add_argument('-v', '--version', action='store_const', const=True, help='Retrieve version info for this file.')
  parser.add_argument('-o', '--output', default='heatmaps_asset_store', help='The name of the output directory. If omitted, default name is heatmaps_asset_store.')
  args = vars(parser.parse_args())

  if 'help' in args:
    parser.print_help()
    sys.exit()
  elif args['version'] == True:
    version_info()
    sys.exit()

  else:
    output_path = "../../" + args['output']
    if (os.path.exists(output_path)):
      shutil.rmtree(output_path)
    os.mkdir(output_path)
    for x in include_these:
      dest = output_path + "/" + x
      src = "../../" + x
      shutil.copytree(src, dest)
    for x in then_exclude_these_directories:
      shutil.rmtree(output_path + "/Heatmaps/" + x)
    for x in then_exclude_these_files:
      os.remove(output_path + "/Heatmaps/" + x)
    print 'success'

if __name__ == "__main__":
   main(sys.argv[1:])
