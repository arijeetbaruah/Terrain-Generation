echo Starting Build Process
'C:\Program Files\Unity\Hub\Editor\2022.3.7f1\Editor\Unity.exe' -quit -batchmode -projectPath ./ -executeMethod BuildScript.PerformBuild
echo Ended Build Process
./butler/butler.exe push ./Builds/PC dragonking1994/terrain-generation:windows
./butler/butler.exe push ./Builds/WebGL dragonking1994/terrain-generation:HTML
