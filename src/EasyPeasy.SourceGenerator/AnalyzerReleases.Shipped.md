; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 3.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EP0001 | EasyPeasy | Error | Method has no HTTP method attribute
EP0002 | EasyPeasy | Error | Unsupported interface member
EP0003 | EasyPeasy | Error | Generic methods are not supported
EP0004 | EasyPeasy | Error | Generic interfaces are not supported
EP0005 | EasyPeasy | Error | Unsupported return type
EP0006 | EasyPeasy | Error | ref, out and in parameters are not supported
EP0007 | EasyPeasy | Error | More than one body parameter
EP0008 | EasyPeasy | Error | Body and form parameters cannot be combined
EP0009 | EasyPeasy | Error | Path variable has no parameter
EP0010 | EasyPeasy | Error | Path parameter is not in the path
EP0011 | EasyPeasy | Error | Parameter has more than one binding attribute
EP0012 | EasyPeasy | Error | Interface is not accessible
EP0013 | EasyPeasy | Error | Method has more than one HTTP method attribute
EP0014 | EasyPeasy | Error | More than one CancellationToken parameter
