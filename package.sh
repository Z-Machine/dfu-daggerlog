#!/bin/bash

# Configuration
SEARCH_DIR="${1:-/home/zed/Github/daggerfall-unity/Assets/Untracked/ModBuilder}"
TARGET_FILE="${2:-daggerlog.dfmod}"
ARCHIVE_NAME="${3:-daggerlog}"

# Verify 7z is installed
if ! command -v 7z &> /dev/null; then
    echo "Error: 7z command not found. Please install 7zip."
    exit 1
fi

# Create a temporary list file for 7z
TEMP_LIST=$(mktemp)

# Find matching files and store them with relative paths in the list file
pushd "$SEARCH_DIR" > /dev/null
find StandaloneLinux64 StandaloneOSX StandaloneWindows -type f -name "$TARGET_FILE" -print > "$TEMP_LIST"
popd > /dev/null

# Check if any files were found
if [ ! -s "$TEMP_LIST" ]; then
    echo "Error: No matching files found."
    rm -f "$TEMP_LIST"
    exit 1
fi

# Determine the JSON file path
JSON_FILE="$(dirname "$0")/${TARGET_FILE}.json"

# Extract ModVersion from JSON if the file exists
if [ -f "$JSON_FILE" ]; then
    MOD_VERSION=$(jq -r '.ModVersion' "$JSON_FILE")
    if [ "$MOD_VERSION" != "null" ]; then
        ARCHIVE_NAME="${ARCHIVE_NAME}-v${MOD_VERSION}.zip"
    else
        echo "Error: \"ModVersion\" not found in JSON."
        exit 1
    fi
else
    echo "Error: JSON file $JSON_FILE not found."
    exit 1
fi

# Remove existing archive if it exists
if [ -f "$ARCHIVE_NAME" ]; then
    echo "Removing existing archive: $ARCHIVE_NAME"
    rm -f "$ARCHIVE_NAME"
fi

# Create the zip archive with maximum compression and relative paths
pushd "$SEARCH_DIR" > /dev/null
7z a -tzip -mx=9 "$OLDPWD/dist/$ARCHIVE_NAME" @$TEMP_LIST
popd > /dev/null

# Clean up
rm -f "$TEMP_LIST"

echo "Archive $ARCHIVE_NAME created successfully with maximum compression."
