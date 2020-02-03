# In case you have issues with CBT or building the project, try this :
# From the root of the repo Compute-CPlat-SALsA
msbuild build\Local\CBTModules\CBTModules.proj  /t:restore
nuget restore src
msbuild src\SALsA.src