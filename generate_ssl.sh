#!/bin/sh

# Generates a default certificate files. If no argument provided,
# it will generate files starting with .MyCertificate.

# Usage example: sh generate_ssl.sh [CUSTOM_CERTIFICATE_FILENAME]

if [ -n "$1" ]; then
    certificate_name="$1"
else
    certificate_name=".MyCertificate"
fi

openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -nodes \
    -days 3650 \
    -keyout "$certificate_name.key" \
    -out "$certificate_name.crt"

openssl pkcs12 \
    -export \
    -inkey "$certificate_name.key" \
    -in "$certificate_name.crt" \
    -out "$certificate_name.pfx"
