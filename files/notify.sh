#!/bin/bash
i=0
p=0

while [ $i -lt 5 ]
do
    i=$[$i + 1]
    echo $((10 * i))
    sleep 1
    p=$[$p + 1]
done > >(zenity --progress --title="Aturada automàtica de l'equip" --text="Aquest equip te programada una aturada automàtica a les <b>05/27/2020 15:37:50</b>.\nSi su plau, desi els treballs en curs i tanqui totes les aplicacions" --percentage=0 --auto-close --auto-kill --time-remaining)
echo 'shutdown'