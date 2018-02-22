# DecryptCocos2dAsset

### How to get key
key size is 16byte(4byte * 4key)

hook or static/dynamic disassemble these functions
```txt
cocos2d::ZipUtils::setPvrEncryptionKeyPart(int,uint)
cocos2d::ZipUtils::setPvrEncryptionKey(uint,uint,uint,uint)
```

example
![](https://i.imgur.com/d3Uvc5O.png)

### Platform
All platform is supported (Windows/Linux/macOS)

### How to run
```
$ dotnet run
```

### Requirements
- [.NET Core 2.0](https://dotnet.github.io/)<br>
