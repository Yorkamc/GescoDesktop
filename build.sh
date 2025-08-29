#!/bin/bash
# Build script para Linux/macOS

echo "🔨 Building GESCO Desktop for production..."

# Build backend
echo "📦 Building backend..."
cd backend
dotnet publish src/Gesco.Desktop.UI/Gesco.Desktop.UI.csproj -c Release -o dist
cd ..

# Build frontend
echo "🎯 Building frontend..."
cd frontend
npm run build
cd ..

echo " Build completed!"
echo "Backend: backend/dist/"
echo "Frontend: frontend/dist/"
