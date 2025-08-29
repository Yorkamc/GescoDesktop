@echo off
REM Build script para Windows

echo  Building GESCO Desktop for production...

REM Build backend
echo  Building backend...
cd backend
dotnet publish src\Gesco.Desktop.UI\Gesco.Desktop.UI.csproj -c Release -o dist
cd ..

REM Build frontend  
echo 🎯 Building frontend...
cd frontend
call npm run build
cd ..

echo ✅ Build completed!
echo Backend: backend\dist\
echo Frontend: frontend\dist\
pause
