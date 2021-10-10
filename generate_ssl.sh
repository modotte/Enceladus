#!/bin/sh

# Generates a default certificate files. If two arguments aren't provided,
# it will generate files starting with .MyCertificate with localhost as
# common name.

# Usage example: sh generate_ssl.sh [CUSTOM_CERTIFICATE_FILENAME] [COMMON_NAME]

if [ -n "$1" ] && [ -n "$2" ]; then
    certificate_name="$1"
    common_name="$2"
else
    certificate_name=".MyCertificate"
    common_name="localhost"
fi

openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -nodes \
    -days 3650 \
    -subj "/C=US/ST=Oregon/L=Portland/CN=$common_name" \
    -keyout "$certificate_name.key" \
    -out "$certificate_name.crt"

openssl pkcs12 \
    -export \
    -inkey "$certificate_name.key" \
    -in "$certificate_name.crt" \
    -out "$certificate_name.pfx"
