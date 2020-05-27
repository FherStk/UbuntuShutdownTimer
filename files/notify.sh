#!/bin/bash
i=0
p=(100/$1)

while [ $i -lt 100 ]
do
    i=$[$i + $p]
    echo $i
    sleep 1
done | zenity --progress --title="$2" --text="$3\n" --percentage=0 --auto-close --auto-kill --width=365 --height=150 $4
echo 'shutdown'
exit 0