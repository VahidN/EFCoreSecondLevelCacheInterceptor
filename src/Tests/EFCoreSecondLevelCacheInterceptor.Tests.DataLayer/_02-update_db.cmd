dotnet tool update --global dotnet-ef --version 9.0.0
dotnet build
dotnet ef --startup-project ../EFCoreSecondLevelCacheInterceptor.AspNetCoreSample/ database update --context ApplicationDbContext
pause