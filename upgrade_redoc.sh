#!/usr/bin/env bash

DOWNLOAD_FOLDER="redoc_upgrade_dowload"
REDOC="redoc.standalone.js"
REDOC_MAP="redoc.standalone.js.map"

if [ -d "$DOWNLOAD_FOLDER" ]; then
  echo "Deleting artifacts from previous upgrade"
  rm -rf $DOWNLOAD_FOLDER
fi

echo "Downloading latest version"
mkdir $DOWNLOAD_FOLDER  
curl https://cdn.jsdelivr.net/npm/redoc/bundles/redoc.standalone.js > $DOWNLOAD_FOLDER/$REDOC
curl https://cdn.jsdelivr.net/npm/redoc/bundles/redoc.standalone.js.map > $DOWNLOAD_FOLDER/$REDOC_MAP

if [ -f "$DOWNLOAD_FOLDER/$REDOC" ] && [ -f "$DOWNLOAD_FOLDER/$REDOC_MAP" ]; then
  echo "Replacing ReDoc with downloaded version"
  mv -f $DOWNLOAD_FOLDER/$REDOC src/Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc/www/redoc.standalone.js   
  mv -f $DOWNLOAD_FOLDER/$REDOC_MAP src/Be.Vlaanderen.Basisregisters.AspNetCore.Swagger.ReDoc/www/redoc.standalone.js.map
  rm -d $DOWNLOAD_FOLDER   
else
  echo "Failed to update ReDoc"
fi