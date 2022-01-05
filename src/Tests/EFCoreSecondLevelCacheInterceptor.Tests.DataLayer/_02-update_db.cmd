dotnet tool update --global dotnet-ef --version 6.0.1
dotnet build
dotnet ef --startup-project ../EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/ database update --context ApplicationDbContext
pause