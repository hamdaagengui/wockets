<?php require_once('../Connections/Wockets.php'); ?>
<?php
// Load the common classes
require_once('../includes/common/KT_common.php');

// Load the required classes
require_once('../includes/tfi/TFI.php');
require_once('../includes/tso/TSO.php');
require_once('../includes/nav/NAV.php');

// Make unified connection variable
$conn_Wockets = new KT_connection($Wockets, $database_Wockets);

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
$tfi_listPARTICIPANT_WOCKETS2 = new TFI_TableFilter($conn_Wockets, "tfi_listPARTICIPANT_WOCKETS2");
$tfi_listPARTICIPANT_WOCKETS2->addColumn("PARTICIPANTS.id", "NUMERIC_TYPE", "participant_id", "=");
$tfi_listPARTICIPANT_WOCKETS2->addColumn("WOCKETS.id", "NUMERIC_TYPE", "wocket_id", "=");
$tfi_listPARTICIPANT_WOCKETS2->Execute();

// Sorter
$tso_listPARTICIPANT_WOCKETS2 = new TSO_TableSorter("rsPARTICIPANT_WOCKETS1", "tso_listPARTICIPANT_WOCKETS2");
$tso_listPARTICIPANT_WOCKETS2->addColumn("PARTICIPANTS.last_name");
$tso_listPARTICIPANT_WOCKETS2->addColumn("WOCKETS.mac");
$tso_listPARTICIPANT_WOCKETS2->setDefault("PARTICIPANT_WOCKETS.participant_id");
$tso_listPARTICIPANT_WOCKETS2->Execute();

// Navigation
$nav_listPARTICIPANT_WOCKETS2 = new NAV_Regular("nav_listPARTICIPANT_WOCKETS2", "rsPARTICIPANT_WOCKETS1", "../", $_SERVER['PHP_SELF'], 10);

mysql_select_db($database_Wockets, $Wockets);
$query_Recordset1 = "SELECT last_name, id FROM PARTICIPANTS ORDER BY last_name";
$Recordset1 = mysql_query($query_Recordset1, $Wockets) or die(mysql_error());
$row_Recordset1 = mysql_fetch_assoc($Recordset1);
$totalRows_Recordset1 = mysql_num_rows($Recordset1);

mysql_select_db($database_Wockets, $Wockets);
$query_Recordset2 = "SELECT mac, id FROM WOCKETS ORDER BY mac";
$Recordset2 = mysql_query($query_Recordset2, $Wockets) or die(mysql_error());
$row_Recordset2 = mysql_fetch_assoc($Recordset2);
$totalRows_Recordset2 = mysql_num_rows($Recordset2);

//NeXTenesio3 Special List Recordset
$maxRows_rsPARTICIPANT_WOCKETS1 = $_SESSION['max_rows_nav_listPARTICIPANT_WOCKETS2'];
$pageNum_rsPARTICIPANT_WOCKETS1 = 0;
if (isset($_GET['pageNum_rsPARTICIPANT_WOCKETS1'])) {
  $pageNum_rsPARTICIPANT_WOCKETS1 = $_GET['pageNum_rsPARTICIPANT_WOCKETS1'];
}
$startRow_rsPARTICIPANT_WOCKETS1 = $pageNum_rsPARTICIPANT_WOCKETS1 * $maxRows_rsPARTICIPANT_WOCKETS1;

// Defining List Recordset variable
$NXTFilter_rsPARTICIPANT_WOCKETS1 = "1=1";
if (isset($_SESSION['filter_tfi_listPARTICIPANT_WOCKETS2'])) {
  $NXTFilter_rsPARTICIPANT_WOCKETS1 = $_SESSION['filter_tfi_listPARTICIPANT_WOCKETS2'];
}
// Defining List Recordset variable
$NXTSort_rsPARTICIPANT_WOCKETS1 = "PARTICIPANT_WOCKETS.participant_id";
if (isset($_SESSION['sorter_tso_listPARTICIPANT_WOCKETS2'])) {
  $NXTSort_rsPARTICIPANT_WOCKETS1 = $_SESSION['sorter_tso_listPARTICIPANT_WOCKETS2'];
}
mysql_select_db($database_Wockets, $Wockets);

$query_rsPARTICIPANT_WOCKETS1 = "SELECT PARTICIPANTS.last_name AS participant_id, WOCKETS.mac AS wocket_id FROM (PARTICIPANT_WOCKETS LEFT JOIN PARTICIPANTS ON PARTICIPANT_WOCKETS.participant_id = PARTICIPANTS.id) LEFT JOIN WOCKETS ON PARTICIPANT_WOCKETS.wocket_id = WOCKETS.id WHERE {$NXTFilter_rsPARTICIPANT_WOCKETS1} ORDER BY {$NXTSort_rsPARTICIPANT_WOCKETS1}";
$query_limit_rsPARTICIPANT_WOCKETS1 = sprintf("%s LIMIT %d, %d", $query_rsPARTICIPANT_WOCKETS1, $startRow_rsPARTICIPANT_WOCKETS1, $maxRows_rsPARTICIPANT_WOCKETS1);
$rsPARTICIPANT_WOCKETS1 = mysql_query($query_limit_rsPARTICIPANT_WOCKETS1, $Wockets) or die(mysql_error());
$row_rsPARTICIPANT_WOCKETS1 = mysql_fetch_assoc($rsPARTICIPANT_WOCKETS1);

if (isset($_GET['totalRows_rsPARTICIPANT_WOCKETS1'])) {
  $totalRows_rsPARTICIPANT_WOCKETS1 = $_GET['totalRows_rsPARTICIPANT_WOCKETS1'];
} else {
  $all_rsPARTICIPANT_WOCKETS1 = mysql_query($query_rsPARTICIPANT_WOCKETS1);
  $totalRows_rsPARTICIPANT_WOCKETS1 = mysql_num_rows($all_rsPARTICIPANT_WOCKETS1);
}
$totalPages_rsPARTICIPANT_WOCKETS1 = ceil($totalRows_rsPARTICIPANT_WOCKETS1/$maxRows_rsPARTICIPANT_WOCKETS1)-1;
//End NeXTenesio3 Special List Recordset

$nav_listPARTICIPANT_WOCKETS2->checkBoundries();
?><!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
<title>Untitled Document</title>
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
  .KT_col_participant_id {width:140px; overflow:hidden;}
  .KT_col_wocket_id {width:140px; overflow:hidden;}
</style>
</head>

<body>
<div class="KT_tng" id="listPARTICIPANT_WOCKETS2">
  <h1> PARTICIPANT_WOCKETS
    <?php
  $nav_listPARTICIPANT_WOCKETS2->Prepare();
  require("../includes/nav/NAV_Text_Statistics.inc.php");
?>
  </h1>
  <div class="KT_tnglist">
    <form action="<?php echo KT_escapeAttribute(KT_getFullUri()); ?>" method="post" id="form1">
      <div class="KT_options"> <a href="<?php echo $nav_listPARTICIPANT_WOCKETS2->getShowAllLink(); ?>"><?php echo NXT_getResource("Show"); ?>
            <?php 
  // Show IF Conditional region1
  if (@$_GET['show_all_nav_listPARTICIPANT_WOCKETS2'] == 1) {
?>
              <?php echo $_SESSION['default_max_rows_nav_listPARTICIPANT_WOCKETS2']; ?>
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
  if (@$_SESSION['has_filter_tfi_listPARTICIPANT_WOCKETS2'] == 1) {
?>
                              <a href="<?php echo $tfi_listPARTICIPANT_WOCKETS2->getResetFilterLink(); ?>"><?php echo NXT_getResource("Reset filter"); ?></a>
                              <?php 
  // else Conditional region2
  } else { ?>
                              <a href="<?php echo $tfi_listPARTICIPANT_WOCKETS2->getShowFilterLink(); ?>"><?php echo NXT_getResource("Show filter"); ?></a>
                              <?php } 
  // endif Conditional region2
?>
      </div>
      <table cellpadding="2" cellspacing="0" class="KT_tngtable">
        <thead>
          <tr class="KT_row_order">
            <th> <input type="checkbox" name="KT_selAll" id="KT_selAll"/>
            </th>
            <th id="participant_id" class="KT_sorter KT_col_participant_id <?php echo $tso_listPARTICIPANT_WOCKETS2->getSortIcon('PARTICIPANTS.last_name'); ?>"> <a href="<?php echo $tso_listPARTICIPANT_WOCKETS2->getSortLink('PARTICIPANTS.last_name'); ?>">Participant_id</a> </th>
            <th id="wocket_id" class="KT_sorter KT_col_wocket_id <?php echo $tso_listPARTICIPANT_WOCKETS2->getSortIcon('WOCKETS.mac'); ?>"> <a href="<?php echo $tso_listPARTICIPANT_WOCKETS2->getSortLink('WOCKETS.mac'); ?>">Wocket_id</a> </th>
            <th>&nbsp;</th>
          </tr>
          <?php 
  // Show IF Conditional region3
  if (@$_SESSION['has_filter_tfi_listPARTICIPANT_WOCKETS2'] == 1) {
?>
            <tr class="KT_row_filter">
              <td>&nbsp;</td>
              <td><select name="tfi_listPARTICIPANT_WOCKETS2_participant_id" id="tfi_listPARTICIPANT_WOCKETS2_participant_id">
                <option value="" <?php if (!(strcmp("", @$_SESSION['tfi_listPARTICIPANT_WOCKETS2_participant_id']))) {echo "SELECTED";} ?>><?php echo NXT_getResource("None"); ?></option>
                <?php
do {  
?>
                <option value="<?php echo $row_Recordset1['id']?>"<?php if (!(strcmp($row_Recordset1['id'], @$_SESSION['tfi_listPARTICIPANT_WOCKETS2_participant_id']))) {echo "SELECTED";} ?>><?php echo $row_Recordset1['last_name']?></option>
                <?php
} while ($row_Recordset1 = mysql_fetch_assoc($Recordset1));
  $rows = mysql_num_rows($Recordset1);
  if($rows > 0) {
      mysql_data_seek($Recordset1, 0);
	  $row_Recordset1 = mysql_fetch_assoc($Recordset1);
  }
?>
              </select>
              </td>
              <td><select name="tfi_listPARTICIPANT_WOCKETS2_wocket_id" id="tfi_listPARTICIPANT_WOCKETS2_wocket_id">
                <option value="" <?php if (!(strcmp("", @$_SESSION['tfi_listPARTICIPANT_WOCKETS2_wocket_id']))) {echo "SELECTED";} ?>><?php echo NXT_getResource("None"); ?></option>
                <?php
do {  
?>
                <option value="<?php echo $row_Recordset2['id']?>"<?php if (!(strcmp($row_Recordset2['id'], @$_SESSION['tfi_listPARTICIPANT_WOCKETS2_wocket_id']))) {echo "SELECTED";} ?>><?php echo $row_Recordset2['mac']?></option>
                <?php
} while ($row_Recordset2 = mysql_fetch_assoc($Recordset2));
  $rows = mysql_num_rows($Recordset2);
  if($rows > 0) {
      mysql_data_seek($Recordset2, 0);
	  $row_Recordset2 = mysql_fetch_assoc($Recordset2);
  }
?>
              </select>
              </td>
              <td><input type="submit" name="tfi_listPARTICIPANT_WOCKETS2" value="<?php echo NXT_getResource("Filter"); ?>" /></td>
            </tr>
            <?php } 
  // endif Conditional region3
?>
        </thead>
        <tbody>
          <?php if ($totalRows_rsPARTICIPANT_WOCKETS1 == 0) { // Show if recordset empty ?>
            <tr>
              <td colspan="4"><?php echo NXT_getResource("The table is empty or the filter you've selected is too restrictive."); ?></td>
            </tr>
            <?php } // Show if recordset empty ?>
          <?php if ($totalRows_rsPARTICIPANT_WOCKETS1 > 0) { // Show if recordset not empty ?>
            <?php do { ?>
              <tr class="<?php echo @$cnt1++%2==0 ? "" : "KT_even"; ?>">
                <td><input type="checkbox" name="kt_pk_PARTICIPANT_WOCKETS" class="id_checkbox" value="<?php echo $row_rsPARTICIPANT_WOCKETS1['participant_id']; ?>" />
                    <input type="hidden" name="participant_id" class="id_field" value="<?php echo $row_rsPARTICIPANT_WOCKETS1['participant_id']; ?>" />
                </td>
                <td><div class="KT_col_participant_id"><?php echo KT_FormatForList($row_rsPARTICIPANT_WOCKETS1['participant_id'], 20); ?></div></td>
                <td><div class="KT_col_wocket_id"><?php echo KT_FormatForList($row_rsPARTICIPANT_WOCKETS1['wocket_id'], 20); ?></div></td>
                <td><a class="KT_edit_link" href="tttt_details.php?participant_id=<?php echo $row_rsPARTICIPANT_WOCKETS1['participant_id']; ?>&amp;KT_back=1"><?php echo NXT_getResource("edit_one"); ?></a> <a class="KT_delete_link" href="#delete"><?php echo NXT_getResource("delete_one"); ?></a> </td>
              </tr>
              <?php } while ($row_rsPARTICIPANT_WOCKETS1 = mysql_fetch_assoc($rsPARTICIPANT_WOCKETS1)); ?>
            <?php } // Show if recordset not empty ?>
        </tbody>
      </table>
      <div class="KT_bottomnav">
        <div>
          <?php
            $nav_listPARTICIPANT_WOCKETS2->Prepare();
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
        <a class="KT_additem_op_link" href="tttt_details.php?KT_back=1" onclick="return nxt_list_additem(this)"><?php echo NXT_getResource("add new"); ?></a> </div>
    </form>
  </div>
  <br class="clearfixplain" />
</div>
<p>&nbsp;</p>
</body>
</html>
<?php
mysql_free_result($Recordset1);

mysql_free_result($Recordset2);

mysql_free_result($rsPARTICIPANT_WOCKETS1);
?>
