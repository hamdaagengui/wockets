<?php require_once('../Connections/Wockets.php'); ?>
<?php
// Load the common classes
require_once('../includes/common/KT_common.php');

// Load the tNG classes
require_once('../includes/tng/tNG.inc.php');

// Load the required classes
require_once('../includes/tfi/TFI.php');
require_once('../includes/tso/TSO.php');
require_once('../includes/nav/NAV.php');

// Make a transaction dispatcher instance
$tNGs = new tNG_dispatcher("../");

// Make unified connection variable
$conn_Wockets = new KT_connection($Wockets, $database_Wockets);

//Start Restrict Access To Page
$restrict = new tNG_RestrictAccess($conn_Wockets, "../");
//Grand Levels: Any
$restrict->Execute();
//End Restrict Access To Page

if (!function_exists("GetSQLValueString")) {
function GetSQLValueString($theValue, $theType, $theDefinedValue = "", $theNotDefinedValue = "") 
{
  $theValue = get_magic_quotes_gpc() ? stripslashes($theValue) : $theValue;

  $theValue = function_exists("mysql_real_escape_string") ? mysql_real_escape_string($theValue) : mysql_escape_string($theValue);

  switch ($theType) {
    case "text":
      $theValue = ($theValue != "") ? "'" . $theValue . "'" : "NULL";
      break;    
    case "long":
    case "int":
      $theValue = ($theValue != "") ? intval($theValue) : "NULL";
      break;
    case "double":
      $theValue = ($theValue != "") ? "'" . doubleval($theValue) . "'" : "NULL";
      break;
    case "date":
      $theValue = ($theValue != "") ? "'" . $theValue . "'" : "NULL";
      break;
    case "defined":
      $theValue = ($theValue != "") ? $theDefinedValue : $theNotDefinedValue;
      break;
  }
  return $theValue;
}
}

// Filter
$tfi_listPARTICIPANTS1 = new TFI_TableFilter($conn_Wockets, "tfi_listPARTICIPANTS1");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.first_name", "STRING_TYPE", "first_name", "%");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.last_name", "STRING_TYPE", "last_name", "%");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.email", "STRING_TYPE", "email", "%");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.phonenumber", "STRING_TYPE", "phonenumber", "%");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.birthyear", "NUMERIC_TYPE", "birthyear", "=");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.gender", "STRING_TYPE", "gender", "%");
$tfi_listPARTICIPANTS1->addColumn("PARTICIPANTS.race", "STRING_TYPE", "race", "%");
$tfi_listPARTICIPANTS1->Execute();

// Sorter
$tso_listPARTICIPANTS1 = new TSO_TableSorter("rsPARTICIPANTS1", "tso_listPARTICIPANTS1");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.first_name");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.last_name");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.email");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.phonenumber");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.birthyear");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.gender");
$tso_listPARTICIPANTS1->addColumn("PARTICIPANTS.race");
$tso_listPARTICIPANTS1->setDefault("PARTICIPANTS.first_name");
$tso_listPARTICIPANTS1->Execute();

// Navigation
$nav_listPARTICIPANTS1 = new NAV_Regular("nav_listPARTICIPANTS1", "rsPARTICIPANTS1", "../", $_SERVER['PHP_SELF'], 10);

//NeXTenesio3 Special List Recordset
$maxRows_rsPARTICIPANTS1 = $_SESSION['max_rows_nav_listPARTICIPANTS1'];
$pageNum_rsPARTICIPANTS1 = 0;
if (isset($_GET['pageNum_rsPARTICIPANTS1'])) {
  $pageNum_rsPARTICIPANTS1 = $_GET['pageNum_rsPARTICIPANTS1'];
}
$startRow_rsPARTICIPANTS1 = $pageNum_rsPARTICIPANTS1 * $maxRows_rsPARTICIPANTS1;

// Defining List Recordset variable
$NXTFilter_rsPARTICIPANTS1 = "1=1";
if (isset($_SESSION['filter_tfi_listPARTICIPANTS1'])) {
  $NXTFilter_rsPARTICIPANTS1 = $_SESSION['filter_tfi_listPARTICIPANTS1'];
}
// Defining List Recordset variable
$NXTSort_rsPARTICIPANTS1 = "PARTICIPANTS.first_name";
if (isset($_SESSION['sorter_tso_listPARTICIPANTS1'])) {
  $NXTSort_rsPARTICIPANTS1 = $_SESSION['sorter_tso_listPARTICIPANTS1'];
}
mysql_select_db($database_Wockets, $Wockets);

$query_rsPARTICIPANTS1 = "SELECT PARTICIPANTS.first_name, PARTICIPANTS.last_name, PARTICIPANTS.email, PARTICIPANTS.phonenumber, PARTICIPANTS.birthyear, PARTICIPANTS.gender, PARTICIPANTS.race, PARTICIPANTS.id FROM PARTICIPANTS WHERE {$NXTFilter_rsPARTICIPANTS1} ORDER BY {$NXTSort_rsPARTICIPANTS1}";
$query_limit_rsPARTICIPANTS1 = sprintf("%s LIMIT %d, %d", $query_rsPARTICIPANTS1, $startRow_rsPARTICIPANTS1, $maxRows_rsPARTICIPANTS1);
$rsPARTICIPANTS1 = mysql_query($query_limit_rsPARTICIPANTS1, $Wockets) or die(mysql_error());
$row_rsPARTICIPANTS1 = mysql_fetch_assoc($rsPARTICIPANTS1);

if (isset($_GET['totalRows_rsPARTICIPANTS1'])) {
  $totalRows_rsPARTICIPANTS1 = $_GET['totalRows_rsPARTICIPANTS1'];
} else {
  $all_rsPARTICIPANTS1 = mysql_query($query_rsPARTICIPANTS1);
  $totalRows_rsPARTICIPANTS1 = mysql_num_rows($all_rsPARTICIPANTS1);
}
$totalPages_rsPARTICIPANTS1 = ceil($totalRows_rsPARTICIPANTS1/$maxRows_rsPARTICIPANTS1)-1;
//End NeXTenesio3 Special List Recordset

// Make a logout transaction instance
$logoutTransaction = new tNG_logoutTransaction($conn_Wockets);
$tNGs->addTransaction($logoutTransaction);
// Register triggers
$logoutTransaction->registerTrigger("STARTER", "Trigger_Default_Starter", 1, "GET", "KT_logout_now");
$logoutTransaction->registerTrigger("END", "Trigger_Default_Redirect", 99, "../index.php");
// Add columns
// End of logout transaction instance

// Execute all the registered transactions
$tNGs->executeTransactions();

$nav_listPARTICIPANTS1->checkBoundries();

// Get the transaction recordset
$rscustom = $tNGs->getRecordset("custom");
$row_rscustom = mysql_fetch_assoc($rscustom);
$totalRows_rscustom = mysql_num_rows($rscustom);
?><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
<title>Untitled Document</title>
<script src="../includes/cssmenus2/js/cssmenus.js" type="text/javascript"></script>
<link href="../includes/cssmenus2/skins/interakt_blue/horizontal.css" rel="stylesheet" type="text/css" />
<script src="../includes/common/js/base.js" type="text/javascript"></script>
<script src="../includes/common/js/utility.js" type="text/javascript"></script>

<link href="../includes/skins/mxkollection3.css" rel="stylesheet" type="text/css" media="all" />
<script src="../includes/common/js/base.js" type="text/javascript"></script>
<script src="../includes/common/js/utility.js" type="text/javascript"></script>
<script src="../includes/skins/style.js" type="text/javascript"></script>
<script src="../includes/nxt/scripts/list.js" type="text/javascript"></script>
<script src="../includes/nxt/scripts/list.js.php" type="text/javascript"></script>
<script type="text/javascript">
$NXT_LIST_SETTINGS = {
  duplicate_buttons: true,
  duplicate_navigation: true,
  row_effects: true,
  show_as_buttons: true,
  record_counter: true
}
</script>
<style type="text/css">
  /* NeXTensio3 List row settings */
  .KT_col_first_name {width:140px; overflow:hidden;}
  .KT_col_last_name {width:140px; overflow:hidden;}
  .KT_col_email {width:140px; overflow:hidden;}
  .KT_col_phonenumber {width:140px; overflow:hidden;}
  .KT_col_birthyear {width:140px; overflow:hidden;}
  .KT_col_gender {width:140px; overflow:hidden;}
  .KT_col_race {width:140px; overflow:hidden;}
</style>
</head>

<body>



<p> </p>
<div id="cssMenu1" class="horizontal" >
  <ul class="interakt_blue">
    <li> <a href="../main.php" title="Logout">Home</a> </li>
    <li> <a href="#" title="Study">Study</a>
        <ul>
          <li> <a href="phones.php" title="Phones">Phones</a> </li>
          <li> <a href="wockets.php" title="Wockets">Wockets</a> </li>
          <li> <a href="participants.php" title="Participants">Participants</a> </li>
      </ul>
    </li>
    <li> <a href="#" title="Export">Actions</a>
        <ul>
          <li> <a href="participant_phone.php" title="Assign phone to participant">Assign Phone</a> </li>
          <li> <a href="participant_wocket.php" title="Assign Wocket">Assign Wocket</a> </li>
          <li> <a href="ExportData.php" title="Export Data">Export Data</a> </li>
      </ul>
    </li>
    <li> <a href="#" title="Advanced">Advanced</a>
        <ul>
          <li> <a href="accounts.php" title="User Accounts">User Accounts</a> </li>
          <li> <a href="Files.php" title="Files">Files</a> </li>
          <li> <a href="phone_stats.php" title="Phone Statistics">Phone Statistics</a> </li>
          <li> <a href="wocket_stats.php" title="Wockets Statistics">Wockets Statistics</a> </li>
      </ul>
    </li>
    <li> <a href="<?php echo $logoutTransaction->getLogoutLink(); ?>" title="">Logout</a> </li>
  </ul>
  <br />
  <script type="text/javascript">
	<!--
    var obj_cssMenu1 = new CSSMenu("cssMenu1");
    obj_cssMenu1.setTimeouts(400, 200, 800);
    obj_cssMenu1.setSubMenuOffset(0, 0, 0, 0);
    obj_cssMenu1.setHighliteCurrent(true);
    obj_cssMenu1.show();
   //-->
  </script>
</div>
<p>&nbsp;</p>


<div class="KT_tng" id="listPARTICIPANTS1">
  <h1> PARTICIPANTS
    <?php
  $nav_listPARTICIPANTS1->Prepare();
  require("../includes/nav/NAV_Text_Statistics.inc.php");
?>
  </h1>
  <div class="KT_tnglist">
    <form action="<?php echo KT_escapeAttribute(KT_getFullUri()); ?>" method="post" id="form1">
      <div class="KT_options"> <a href="<?php echo $nav_listPARTICIPANTS1->getShowAllLink(); ?>"><?php echo NXT_getResource("Show"); ?>
        <?php 
  // Show IF Conditional region1
  if (@$_GET['show_all_nav_listPARTICIPANTS1'] == 1) {
?>
          <?php echo $_SESSION['default_max_rows_nav_listPARTICIPANTS1']; ?>
          <?php 
  // else Conditional region1
  } else { ?>
          <?php echo NXT_getResource("all"); ?>
          <?php } 
  // endif Conditional region1
?>
            <?php echo NXT_getResource("records"); ?></a> &nbsp;
        &nbsp;
                            <?php 
  // Show IF Conditional region2
  if (@$_SESSION['has_filter_tfi_listPARTICIPANTS1'] == 1) {
?>
                              <a href="<?php echo $tfi_listPARTICIPANTS1->getResetFilterLink(); ?>"><?php echo NXT_getResource("Reset filter"); ?></a>
                              <?php 
  // else Conditional region2
  } else { ?>
                              <a href="<?php echo $tfi_listPARTICIPANTS1->getShowFilterLink(); ?>"><?php echo NXT_getResource("Show filter"); ?></a>
                              <?php } 
  // endif Conditional region2
?>
      </div>
      <table cellpadding="2" cellspacing="0" class="KT_tngtable">
        <thead>
          <tr class="KT_row_order">
            <th> <input type="checkbox" name="KT_selAll" id="KT_selAll"/>
            </th>
            <th id="first_name" class="KT_sorter KT_col_first_name <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.first_name'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.first_name'); ?>">First Name</a> </th>
            <th id="last_name" class="KT_sorter KT_col_last_name <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.last_name'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.last_name'); ?>">Last Name</a> </th>
            <th id="email" class="KT_sorter KT_col_email <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.email'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.email'); ?>">Email</a> </th>
            <th id="phonenumber" class="KT_sorter KT_col_phonenumber <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.phonenumber'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.phonenumber'); ?>">Phone Number</a> </th>
            <th id="birthyear" class="KT_sorter KT_col_birthyear <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.birthyear'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.birthyear'); ?>">Birthyear</a> </th>
            <th id="gender" class="KT_sorter KT_col_gender <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.gender'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.gender'); ?>">Gender</a> </th>
            <th id="race" class="KT_sorter KT_col_race <?php echo $tso_listPARTICIPANTS1->getSortIcon('PARTICIPANTS.race'); ?>"> <a href="<?php echo $tso_listPARTICIPANTS1->getSortLink('PARTICIPANTS.race'); ?>">Race</a> </th>
            <th>&nbsp;</th>
          </tr>
          <?php 
  // Show IF Conditional region3
  if (@$_SESSION['has_filter_tfi_listPARTICIPANTS1'] == 1) {
?>
            <tr class="KT_row_filter">
              <td>&nbsp;</td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_first_name" id="tfi_listPARTICIPANTS1_first_name" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_first_name']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_last_name" id="tfi_listPARTICIPANTS1_last_name" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_last_name']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_email" id="tfi_listPARTICIPANTS1_email" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_email']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_phonenumber" id="tfi_listPARTICIPANTS1_phonenumber" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_phonenumber']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_birthyear" id="tfi_listPARTICIPANTS1_birthyear" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_birthyear']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_gender" id="tfi_listPARTICIPANTS1_gender" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_gender']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listPARTICIPANTS1_race" id="tfi_listPARTICIPANTS1_race" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listPARTICIPANTS1_race']); ?>" size="20" maxlength="100" /></td>
              <td><input type="submit" name="tfi_listPARTICIPANTS1" value="<?php echo NXT_getResource("Filter"); ?>" /></td>
            </tr>
            <?php } 
  // endif Conditional region3
?>
        </thead>
        <tbody>
          <?php if ($totalRows_rsPARTICIPANTS1 == 0) { // Show if recordset empty ?>
            <tr>
              <td colspan="9"><?php echo NXT_getResource("The table is empty or the filter you've selected is too restrictive."); ?></td>
            </tr>
            <?php } // Show if recordset empty ?>
          <?php if ($totalRows_rsPARTICIPANTS1 > 0) { // Show if recordset not empty ?>
            <?php do { ?>
              <tr class="<?php echo @$cnt1++%2==0 ? "" : "KT_even"; ?>">
                <td><input type="checkbox" name="kt_pk_PARTICIPANTS" class="id_checkbox" value="<?php echo $row_rsPARTICIPANTS1['id']; ?>" />
                    <input type="hidden" name="id" class="id_field" value="<?php echo $row_rsPARTICIPANTS1['id']; ?>" />
                </td>
                <td><div class="KT_col_first_name"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['first_name'], 20); ?></div></td>
                <td><div class="KT_col_last_name"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['last_name'], 20); ?></div></td>
                <td><div class="KT_col_email"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['email'], 20); ?></div></td>
                <td><div class="KT_col_phonenumber"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['phonenumber'], 20); ?></div></td>
                <td><div class="KT_col_birthyear"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['birthyear'], 20); ?></div></td>
                <td><div class="KT_col_gender"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['gender'], 20); ?></div></td>
                <td><div class="KT_col_race"><?php echo KT_FormatForList($row_rsPARTICIPANTS1['race'], 20); ?></div></td>
                <td><a class="KT_edit_link" href="participants_details.php?id=<?php echo $row_rsPARTICIPANTS1['id']; ?>&amp;KT_back=1"><?php echo NXT_getResource("edit_one"); ?></a> <a class="KT_delete_link" href="#delete"><?php echo NXT_getResource("delete_one"); ?></a> </td>
              </tr>
              <?php } while ($row_rsPARTICIPANTS1 = mysql_fetch_assoc($rsPARTICIPANTS1)); ?>
            <?php } // Show if recordset not empty ?>
        </tbody>
      </table>
      <div class="KT_bottomnav">
        <div>
          <?php
            $nav_listPARTICIPANTS1->Prepare();
            require("../includes/nav/NAV_Text_Navigation.inc.php");
          ?>
        </div>
      </div>
      <div class="KT_bottombuttons">
        <div class="KT_operations"> <a class="KT_edit_op_link" href="#" onclick="nxt_list_edit_link_form(this); return false;"><?php echo NXT_getResource("edit_all"); ?></a> <a class="KT_delete_op_link" href="#" onclick="nxt_list_delete_link_form(this); return false;"><?php echo NXT_getResource("delete_all"); ?></a> </div>
<span>&nbsp;</span>
        <select name="no_new" id="no_new">
          <option value="1">1</option>
          <option value="3">3</option>
          <option value="6">6</option>
        </select>
        <a class="KT_additem_op_link" href="participants_details.php?KT_back=1" onclick="return nxt_list_additem(this)"><?php echo NXT_getResource("add new"); ?></a> </div>
    </form>
  </div>
  <br class="clearfixplain" />
</div>
<p>&nbsp;</p>
</body>
</html>
<?php
mysql_free_result($rsPARTICIPANTS1);
?>
