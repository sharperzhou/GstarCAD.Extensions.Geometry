@echo off
@rem If 'msbuild' is not recognized, please set compilation environment firstly

echo ===^> Build solution with GstarCAD 2024 or greater ...
msbuild -p:GstarCAD24=True -p:Configuration=Release -p:Platform="Any CPU" -t:Restore -t:Rebuild -v:minimal

echo ===^> Build solution with GstarCAD 2023 or less ...
msbuild -p:GstarCAD24=False -p:Configuration=Release -p:Platform="Any CPU" -t:Restore -t:Rebuild -v:minimal
