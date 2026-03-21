magick -background none icon.svg -define icon:auto-resize=24 Icon24.ico
magick -background none icon.svg -define icon:auto-resize=16 Icon16.ico
magick -background none icon.svg -define icon:auto-resize=16,24,32,48,64,128,256 Logo.ico

move /Y "*.ico" "..\SwitchyLingus.UI\"