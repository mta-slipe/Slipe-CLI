#!/bin/bash

# Copy to var/slipe
cp ./Slipe /var/Slipe -R

# Create slipe bash file to run dotnet command
printf '#!/bin/bash\ndotnet /var/Slipe/Slipe.dll "$@"' > "/bin/slipe"
chmod +x /bin/slipe