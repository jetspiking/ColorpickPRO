<?php
// Database configuration
$dbHost     = 'redacted';
$dbUsername = 'redacted';
$dbPassword = 'redacted';
$dbName     = 'redacted';

// Connect to the database
$conn = new mysqli($dbHost, $dbUsername, $dbPassword, $dbName);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Check if the form is submitted
if ($_SERVER["REQUEST_METHOD"] == "POST" && isset($_POST['architecture']) && isset($_POST['key'])) {
    $architecture = $_POST['architecture'];
    $inputKey = $_POST['key'];

    // Prevent SQL injection
    $inputKey = $conn->real_escape_string($inputKey);

    // Check the identifier in the database
    $query = "SELECT valid FROM download WHERE identifier = '$inputKey'";
    $result = $conn->query($query);

    if ($result->num_rows > 0) {
        $row = $result->fetch_assoc();
        if ($row['valid'] > 0) {
            // Identifier is valid, subtract one from valid
            $newValid = $row['valid'] - 1;
            $updateQuery = "UPDATE download SET valid = '$newValid' WHERE identifier = '$inputKey'";
            $conn->query($updateQuery);

            // Determine the correct file based on architecture
            $fileName = $architecture === 'x64' ? 'ColorpickPRO_x64.exe' : 'ColorpickPRO_x86.exe';

            // Set headers to trigger download
            header('Content-Type: application/octet-stream');
            header("Content-Transfer-Encoding: Binary");
            header("Content-disposition: attachment; filename=\"" . basename($fileName) . "\"");
            readfile($fileName); // Read the file from the same directory as the script
            exit;
        } else {
            echo "No more downloads available for this key.";
        }
    } else {
        echo "Invalid key.";
    }
}

// Close the database connection
$conn->close();
?>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Colorpick - PRO</title>
    <style>
        body {
            text-align: center; /* Center align everything in the body */
            font-family: Arial, sans-serif; /* Set a default font */
        }

        h1 {
            font-size: 36px; /* Increased font size for title */
            margin-bottom: 20px; /* Provides space below the title */
        }

        img {
            width: 150px; /* Set a specific width to display the image larger */
            height: auto; /* Maintain aspect ratio */
            display: block; /* Allows margin to be applied properly */
            margin: 0 auto; /* Centers image */
        }

        .content-container {
            margin: 0 auto; /* Centers the container */
            max-width: 600px; /* Maximum width of the container */
            padding: 0 20px; /* Padding on the sides */
        }

        .download-button {
            padding: 15px 30px; /* Increased padding for larger buttons */
            font-size: 18px; /* Increased font size for button text */
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin: 10px;
        }

        .key-input {
            display: block; /* Makes input block-level for easy centering */
            margin: 20px auto; /* Centers input and provides space above and below */
            padding: 10px;
            font-size: 18px; /* Increased font size for input text */
            width: 80%; /* Set a maximum width for the input field */
            max-width: 300px; /* Ensures input field is not too wide on larger screens */
        }

        .drm-text {
            font-size: 14px; /* Font size for the DRM text */
            text-align: justify; /* Justify text for better readability */
            margin-top: 20px; /* Space above the DRM text */
        }

        .copyright {
            font-size: 14px; /* Font size for the copyright text */
            margin-top: 40px; /* Space above the copyright text */
        }

        form {
            margin-top: 30px; /* Adjusted margin for overall spacing */
        }
    </style>
</head>
<body>

    <div class="content-container">
        <h1>Colorpick - PRO</h1>
        <img src="colorpicklogo.png" alt="Colorpick Logo" id="colorpick-logo">
        
        <!-- Download buttons -->
        <form action="" method="post">
            <input type="submit" name="architecture" value="x86" class="download-button">
            <input type="submit" name="architecture" value="x64" class="download-button">
            <br>
            <input type="text" name="key" placeholder="Enter your key here" class="key-input" required>
        </form>
        
        <p class="drm-text">
            This software is DRM-free. It is allowed to share this application with your friends. 
            However, it is not allowed to upload or sell our paid software. The count of downloads per key is limited, 
            please secure your copy of the software after downloading.
        </p>
    </div>
    
    <!-- Copyright notice -->
    <div class="copyright">
        Colorpick - PRO &copy; 2024
    </div>

</body>
</html>

