
# Generate SSL key

1. Use `makecert` command of vs2015 generate cert file, cert name is `MyTest`.
```
d:> makecert -sv MyTestCert.pvk -n "CN=MyTest" MyTestCert.cer -b 04/20/2016 -e 04/20/2018 -r
```

2. Use `PVK2PFX` command generate `*.pfx` file, password is `123456`
```
d:> PVK2PFX -pvk MyTestCert.pvk -spc MyTestCert.cer  -pfx MyTestCert.pfx -po 123456
```

