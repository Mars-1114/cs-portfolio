<?php
    include "database_connection.php";

    $mode = $_REQUEST['m'];
    switch ($mode) {
        case 'search':
            $sort = $_GET['sort'] ?? 'points';  
            $order = $_GET['order'] ?? 'DESC'; 
            
            $query = "SELECT country, country_noc,
                            SUM(gold) AS gold, SUM(silver) AS silver, SUM(bronze) AS bronze,
                            SUM(gold * 3 + silver * 2 + bronze) AS points
                    FROM Medal
                    GROUP BY country, country_noc";
            
            $query .= " ORDER BY $sort $order"; 
            
            $result = mysqli_query($conn, $query);
            
            if ($result->num_rows > 0) {
                $rank = 1; 
                while ($row = mysqli_fetch_assoc($result)) {
                    echo "<tr class='row' data-value='" . $row['country_noc'] . "'>";
                    echo "<td>" . $rank++ . "</td>"; 
                    echo "<td>" . htmlspecialchars($row['country']) . "</td>";
                    echo "<td>" . htmlspecialchars($row['gold']) . "</td>";
                    echo "<td>" . htmlspecialchars($row['silver']) . "</td>";
                    echo "<td>" . htmlspecialchars($row['bronze']) . "</td>";
                    echo "</tr>";
                }
            } else {
                echo "<tr><td colspan='5'>No data found.</td></tr>";
            }
            break;
        case 'profile':
            $noc = $_REQUEST['c'];
            $sql = "SELECT DISTINCT edition, sport, event, edition_id, result_id, medal
                    FROM Details
                    WHERE country_noc = '$noc' AND medal != ''
                    ORDER BY edition, sport, event";
            $result = $conn->query($sql);

            // store query to structured array
            $arr = [];
            while ($row = $result->fetch_assoc()) {
                if (!array_key_exists($row["edition_id"], $arr)) {
                    $arr[$row["edition_id"]] = [
                        "edition" => $row["edition"],
                        "sports" => []
                    ];
                }
                $arr[$row["edition_id"]]["sports"][$row["sport"]][$row["event"]][] = $row["medal"];
            }

            // get country name
            $sql = "SELECT country
                    FROM Country
                    WHERE noc = '$noc'";
            $country = $conn->query($sql)->fetch_assoc()["country"];
            
            // build html
            // title
            echo "<h2 style='letter-spacing: 1.5px'>$country Medal History</h2>";
            echo "<div id='country-profile'>";
            // each year
            foreach ($arr as $year) {
                $str = "";
                // collapsible button
                $edition = $year["edition"];
                $str .= "<button class='country-profile-collapse-btn'>
                            <span class='material-symbols-outlined collapse-btn-indicator'>
                                keyboard_arrow_down
                            </span>
                            ".$year["edition"]."
                        </button>
                        <div class='country-profile-collapse-content'>";
                // each sport category
                foreach ($year["sports"] as $sport => $events) {
                    // collapsible button
                    $str .= "<button class='country-sport-collapse-btn'>
                                <span class='material-symbols-outlined collapse-btn-indicator'>
                                    keyboard_arrow_down
                                </span>
                                <span>$sport</span>
                            </button>
                            <div class='country-sport-collapse-content'>";
                    // each event
                    foreach ($events as $event => $medals) {
                        // sort medal
                        usort($medals, function($a, $b) {
                            $table = [
                                "Gold" => 2,
                                "Silver" => 1,
                                "Bronze" => 0
                            ];
                            return $table[$b] - $table[$a];
                        });

                        // each medal
                        $str .= "<div style='border-bottom: 1px solid rgb(197, 197, 197); width: 500px'>";
                        foreach ($medals as $medal) {
                            $str .= "<p class='country-event''>";
                            if ($medal == "Gold") {
                                $str .= "<span class='medal small gold'>1</span>";
                            }
                            else if ($medal == "Silver") {
                                $str .= "<span class='medal small silver'>2</span>";
                            }
                            else if ($medal == "Bronze") {
                                $str .= "<span class='medal small bronze'>3</span>";
                            }
                            $str .= " $event</p>";
                        }
                        $str .= "</div>";
                    }
                    $str .= "</div>";
                }
                $str .= "</div>";
                echo $str;
            }
            echo "</div>";
    }

    // close connection
    $conn->close();

?>
