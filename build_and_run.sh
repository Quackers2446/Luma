#!/bin/bash
# Cozy AR Companion Build & Run CLI Automation Script

UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/quackers/dev/Luma/frontend"

# Check if Unity Editor is currently holding the project lock
if ps aux | grep -i "Unity.app/Contents/MacOS/Unity" | grep -v grep | grep -v "batchmode" > /dev/null; then
    echo "=========================================================="
    echo "WARNING: Unity Editor is currently open on this project."
    echo "=========================================================="
    echo "To compile from the command line, please CLOSE the Unity"
    echo "Editor first so the compiler can acquire the project lock."
    echo ""
    echo "Alternative: You can just click the new menu item directly"
    echo "inside your open Unity Editor: Tools > Cozy AR > Setup, Build & Run"
    exit 1
fi

echo "Starting Setup, Build & Run sequence..."
echo "Compiling and deploying to connected Android device..."

# Execute build method in batch mode and stream log to stdout
"$UNITY_PATH" -projectPath "$PROJECT_PATH" \
              -executeMethod CozyAR.Editor.ProjectSetup.RunSetupBuildAndRun \
              -batchmode \
              -quit \
              -logFile -

if [ $? -eq 0 ]; then
    echo "Success! The build completed and has been launched on your device."
else
    echo "Build failed. Please inspect the log output above for compiling errors."
    exit 1
fi
