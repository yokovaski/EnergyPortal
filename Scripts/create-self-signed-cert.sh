read -p "File name of cert: "  filename

pem="$filename.pem"
pfx="$filename.pfx"
key="$filename.key"
csr="$filename.csr"
cert="$filename.cert"

openssl req -x509 -days 9999 -newkey rsa:2048 -keyout $pem -out $pem
openssl pkcs12 -export -in $pem -inkey $pem -out $pfx
openssl genrsa -out $key 2048
openssl req -new -key $key -out $csr
openssl x509 -req -days 9999 -in $csr -signkey $key -out $cert
