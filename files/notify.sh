#!/bin/bash
(
    i=0
    p=$(echo $1/100 | bc -l)

    while [ $i -lt 100 ]
    do
        i=$[$i + 1]
        echo $i
        sleep $p
    done
) |
zenity --progress --title="$2" --text="$3" --percentage=0 --auto-close --width=365 --height=150 $4 

if [ "$?" = 1 ] ; then
    echo "CANCEL"
else
    echo "ACCEPT"
fi