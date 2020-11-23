"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" stop
"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe" start
dotnet run --project "../../das-employer-config-updater/das-employer-config-updater/das-employer-config-updater" choices="build-config-updater.choices.json"