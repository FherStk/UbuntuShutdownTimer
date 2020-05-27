#!/bin/bash
i=0
p=0

while [ $i -lt $1 ]
do
    i=$[$i + 1]
    echo $i
    sleep 1
    p=$[$p + 1]
done > >(zenity --progress --title="$2" --text="$3\n" --percentage=0 --auto-close --auto-kill --width=300 --height=150 $4)
echo 'shutdown'
exit 0