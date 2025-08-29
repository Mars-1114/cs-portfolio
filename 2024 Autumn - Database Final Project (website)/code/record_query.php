<?php
    include('database_connection.php');

    $sql = "SELECT sport AS event, c.country AS country, name, grade, year
            FROM AthleteRecords AS a, Country AS c
            WHERE a.country = c.noc";
    $result = $conn->query($sql);

    while ($row = $result->fetch_assoc()) {
        $grade = explode("(", $row["grade"])[0];
        $unit = rtrim(explode("(", $row["grade"])[1], ")");
        if ($unit == "s") {
            $second = 0;
            $minute = 0;
            $hour = 0;
            if ($grade >= 60) {
                $second = fmod($grade, 60);
                $grade = ($grade - $second) / 60;
                if ($grade >= 60) {
                    $minute = fmod($grade, 60);
                    $hour = ($grade - $minute) / 60;
                }
            }
            else {
                $second = $grade;
            }
            if ($hour > 0) {
                $grade = "$hour h, $minute m, $second s";
            }
            else if ($minute > 0) {
                $grade = "$minute m, $second s";
            }
            else {
                $grade = "$second s";
            }
        }
        else {
            $grade = "$grade $unit";
        }
        echo '<tr>
                <td>' . $row['event'] . '</td>
                <td>' . $row['name'] . '</td>
                <td>' . $row['country'] . '</td>
                <td>' . $grade . '</td>
                <td>' . $row['year'] . '</td>
            </tr>';
    }

    // close connection
    $conn->close();
?>
