#!/bin/sh

tar_url="$1"
main_lib="$2"
asset_name="$3"
auth_header="$4"

if [ -z "$tar_url" ]
then
	echo "Parameter tar_url is not specified" >&2
	exit 1
fi

if [ -z "$main_lib" ]
then
	echo "Parameter main_lib is not specified" >&2
	exit 1
fi

if [ -z "$asset_name" ]
then
	echo "Parameter asset_name is not specified" >&2
	exit 1
fi

echo Downloading asset file from URL: $tar_url

if [ -z "$auth_header" ]
then
	wget -O ~/tmp.tar $tar_url || exit 1
else
	wget -O ~/tmp.tar --header="$4" $tar_url || exit 1
fi

asset_dir="/etc/task-runtime/assets/$asset_name"

echo Create asset dir $asset_dir
mkdir -p "$asset_dir"

echo Extract tar file to asset dir
tar -xzf ~/tmp.tar -C "$asset_dir"  || exit 1

echo Remove temporary tar file
rm ~/tmp.tar

echo Rename main asset lib file
mv "$asset_dir/$main_lib.dll" "$asset_dir/$asset_name.dll"  || exit 1

echo Completed