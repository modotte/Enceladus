#!/bin/sh

openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -nodes \
    -days 3650 \
    -keyout .MyCertificate.key \
    -out .MyCertificate.crt

openssl pkcs12 \
    -export \
    -inkey .MyCertificate.key \
    -in .MyCertificate.crt \
    -out .MyCertificate.pfx
