#!/bin/bash
# Build script for Zyre.Net C# bindings

set -e

echo "Building Zyre.Net C# bindings..."
echo "================================"

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 8 SDK or later."
    exit 1
fi

# Show .NET version
echo "Using .NET SDK version:"
dotnet --version
echo

# Restore dependencies
echo "Restoring NuGet packages..."
dotnet restore
echo

# Build the solution
echo "Building solution..."
dotnet build --configuration Release --no-restore
echo

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --verbosity minimal
echo

# Run acceptance tests
echo "Running acceptance tests..."
python3 acceptance_test.py
echo

# Create NuGet package
echo "Creating NuGet package..."
dotnet pack Zyre.Net --configuration Release --no-build --output ./packages
echo

echo "Build completed successfully!"
echo "NuGet packages are available in ./packages/"
echo

# Show generated files
echo "Generated files:"
find bin -name "*.dll" -o -name "*.exe" | head -10
echo
find packages -name "*.nupkg" 2>/dev/null || echo "No NuGet packages found"