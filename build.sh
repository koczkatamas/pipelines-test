set -x

echo "Downloading depot_tools"
export PATH=$PATH:$(pwd)/depot_tools
curl --remote-name https://storage.googleapis.com/chrome-infra/depot_tools.zip || { echo Downloading depot_tools failed; exit 1; }
unzip -d depot_tools -o depot_tools.zip || { echo Unzipping depot_tools failed; exit 1; }
cd depot_tools
git checkout . # ninja.exe stucks somehow and update_depot_tools fails otherwise
cd ..

echo "Downloading PDFium"
export DEPOT_TOOLS_WIN_TOOLCHAIN=0
gclient config --unmanaged https://pdfium.googlesource.com/pdfium.git || { echo Gclient config failed; exit 1; }
gclient sync || true # WARNING it does not find python, it should be run from cmd

echo "Downloading rc.exe"
cd pdfium
download_from_google_storage -b chromium-browser-clang/rc -d build/toolchain/win/rc/win

echo "Running gn gen"
gn gen out/Release || { echo Non-configured gn gen failed; exit 1; }

echo "Applying patches"
cp pdfiumviewer.cpp fpdfsdk/ || { echo Could not copy pdfiumviewer.cpp; exit 1; }
cp pdfium.rc fpdfsdk/ || { echo Could not copy pdfium.rc; exit 1; }
cp pdfium-args.gn out/Release/args.gn || { echo Could not copy args.gn; exit 1; }

echo "Applying build.gn patch"
cscs pdfium-build-gn-patch.cs || { echo Could modify BUILD.gn; exit 1; }

echo "Running gn gen again"
gn gen out/Release || { echo Custom configured gn gen failed; exit 1; }

echo "Running ninja"
ninja -C out/Release pdfium_dll || { echo Ninja build failed; exit 1; }
