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
$tfi_listWOCKETS1 = new TFI_TableFilter($conn_Wockets, "tfi_listWOCKETS1");
$tfi_listWOCKETS1->addColumn("WOCKETS.mac", "STRING_TYPE", "mac", "%");
$tfi_listWOCKETS1->addColumn("WOCKETS.color", "STRING_TYPE", "color", "%");
$tfi_listWOCKETS1->addColumn("WOCKETS.hardware", "STRING_TYPE", "hardware", "%");
$tfi_listWOCKETS1->addColumn("WOCKETS.firmware", "STRING_TYPE", "firmware", "%");
$tfi_listWOCKETS1->Execute();

// Sorter
$tso_listWOCKETS1 = new TSO_TableSorter("rsWOCKETS1", "tso_listWOCKETS1");
$tso_listWOCKETS1->addColumn("WOCKETS.mac");
$tso_listWOCKETS1->addColumn("WOCKETS.color");
$tso_listWOCKETS1->addColumn("WOCKETS.hardware");
$tso_listWOCKETS1->addColumn("WOCKETS.firmware");
$tso_listWOCKETS1->setDefault("WOCKETS.mac");
$tso_listWOCKETS1->Execute();

// Navigation
$nav_listWOCKETS1 = new NAV_Regular("nav_listWOCKETS1", "rsWOCKETS1", "../", $_SERVER['PHP_SELF'], 10);

//NeXTenesio3 Special List Recordset
$maxRows_rsWOCKETS1 = $_SESSION['max_rows_nav_listWOCKETS1'];
$pageNum_rsWOCKETS1 = 0;
if (isset($_GET['pageNum_rsWOCKETS1'])) {
  $pageNum_rsWOCKETS1 = $_GET['pageNum_rsWOCKETS1'];
}
$startRow_rsWOCKETS1 = $pageNum_rsWOCKETS1 * $maxRows_rsWOCKETS1;

// Defining List Recordset variable
$NXTFilter_rsWOCKETS1 = "1=1";
if (isset($_SESSION['filter_tfi_listWOCKETS1'])) {
  $NXTFilter_rsWOCKETS1 = $_SESSION['filter_tfi_listWOCKETS1'];
}
// Defining List Recordset variable
$NXTSort_rsWOCKETS1 = "WOCKETS.mac";
if (isset($_SESSION['sorter_tso_listWOCKETS1'])) {
  $NXTSort_rsWOCKETS1 = $_SESSION['sorter_tso_listWOCKETS1'];
}
mysql_select_db($database_Wockets, $Wockets);

$query_rsWOCKETS1 = "SELECT WOCKETS.mac, WOCKETS.color, WOCKETS.hardware, WOCKETS.firmware, WOCKETS.id FROM WOCKETS WHERE {$NXTFilter_rsWOCKETS1} ORDER BY {$NXTSort_rsWOCKETS1}";
$query_limit_rsWOCKETS1 = sprintf("%s LIMIT %d, %d", $query_rsWOCKETS1, $startRow_rsWOCKETS1, $maxRows_rsWOCKETS1);
$rsWOCKETS1 = mysql_query($query_limit_rsWOCKETS1, $Wockets) or die(mysql_error());
$row_rsWOCKETS1 = mysql_fetch_assoc($rsWOCKETS1);

if (isset($_GET['totalRows_rsWOCKETS1'])) {
  $totalRows_rsWOCKETS1 = $_GET['totalRows_rsWOCKETS1'];
} else {
  $all_rsWOCKETS1 = mysql_query($query_rsWOCKETS1);
  $totalRows_rsWOCKETS1 = mysql_num_rows($all_rsWOCKETS1);
}
$totalPages_rsWOCKETS1 = ceil($totalRows_rsWOCKETS1/$maxRows_rsWOCKETS1)-1;
//End NeXTenesio3 Special List Recordset

$nav_listWOCKETS1->checkBoundries();
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
  .KT_col_mac {width:140px; overflow:hidden;}
  .KT_col_color {width:140px; overflow:hidden;}
  .KT_col_hardware {width:140px; overflow:hidden;}
  .KT_col_firmware {width:140px; overflow:hidden;}
</style>
</head>

<body>
<a href="index.php">Admin Home</a>
<div class="KT_tng" id="listWOCKETS1">
  <h1> WOCKETS
    <?php
  $nav_listWOCKETS1->Prepare();
  require("../includes/nav/NAV_Text_Statistics.inc.php");
?>
  </h1>
  <div class="KT_tnglist">
    <form action="<?php echo KT_escapeAttribute(KT_getFullUri()); ?>" method="post" id="form1">
      <div class="KT_options"> <a href="<?php echo $nav_listWOCKETS1->getShowAllLink(); ?>"><?php echo NXT_getResource("Show"); ?>
            <?php 
  // Show IF Conditional region1
  if (@$_GET['show_all_nav_listWOCKETS1'] == 1) {
?>
              <?php echo $_SESSION['default_max_rows_nav_listWOCKETS1']; ?>
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
  if (@$_SESSION['has_filter_tfi_listWOCKETS1'] == 1) {
?>
                              <a href="<?php echo $tfi_listWOCKETS1->getResetFilterLink(); ?>"><?php echo NXT_getResource("Reset filter"); ?></a>
                              <?php 
  // else Conditional region2
  } else { ?>
                              <a href="<?php echo $tfi_listWOCKETS1->getShowFilterLink(); ?>"><?php echo NXT_getResource("Show filter"); ?></a>
                              <?php } 
  // endif Conditional region2
?>
      </div>
      <table cellpadding="2" cellspacing="0" class="KT_tngtable">
        <thead>
          <tr class="KT_row_order">
            <th> <input type="checkbox" name="KT_selAll" id="KT_selAll"/>
            </th>
            <th id="mac" class="KT_sorter KT_col_mac <?php echo $tso_listWOCKETS1->getSortIcon('WOCKETS.mac'); ?>"> <a href="<?php echo $tso_listWOCKETS1->getSortLink('WOCKETS.mac'); ?>">MAC</a> </th>
            <th id="color" class="KT_sorter KT_col_color <?php echo $tso_listWOCKETS1->getSortIcon('WOCKETS.color'); ?>"> <a href="<?php echo $tso_listWOCKETS1->getSortLink('WOCKETS.color'); ?>">Color</a> </th>
            <th id="hardware" class="KT_sorter KT_col_hardware <?php echo $tso_listWOCKETS1->getSortIcon('WOCKETS.hardware'); ?>"> <a href="<?php echo $tso_listWOCKETS1->getSortLink('WOCKETS.hardware'); ?>">Hardware Version</a> </th>
            <th id="firmware" class="KT_sorter KT_col_firmware <?php echo $tso_listWOCKETS1->getSortIcon('WOCKETS.firmware'); ?>"> <a href="<?php echo $tso_listWOCKETS1->getSortLink('WOCKETS.firmware'); ?>">Firmware Version</a> </th>
            <th>&nbsp;</th>
          </tr>
          <?php 
  // Show IF Conditional region3
  if (@$_SESSION['has_filter_tfi_listWOCKETS1'] == 1) {
?>
            <tr class="KT_row_filter">
              <td>&nbsp;</td>
              <td><input type="text" name="tfi_listWOCKETS1_mac" id="tfi_listWOCKETS1_mac" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listWOCKETS1_mac']); ?>" size="20" maxlength="45" /></td>
              <td><input type="text" name="tfi_listWOCKETS1_color" id="tfi_listWOCKETS1_color" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listWOCKETS1_color']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listWOCKETS1_hardware" id="tfi_listWOCKETS1_hardware" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listWOCKETS1_hardware']); ?>" size="20" maxlength="100" /></td>
              <td><input type="text" name="tfi_listWOCKETS1_firmware" id="tfi_listWOCKETS1_firmware" value="<?php echo KT_escapeAttribute(@$_SESSION['tfi_listWOCKETS1_firmware']); ?>" size="20" maxlength="100" /></td>
              <td><input type="submit" name="tfi_listWOCKETS1" value="<?php echo NXT_getResource("Filter"); ?>" /></td>
            </tr>
            <?php } 
  // endif Conditional region3
?>
        </thead>
        <tbody>
          <?php if ($totalRows_rsWOCKETS1 == 0) { // Show if recordset empty ?>
            <tr>
              <td colspan="6"><?php echo NXT_getResource("The table is empty or the filter you've selected is too restrictive."); ?></td>
            </tr>
            <?php } // Show if recordset empty ?>
          <?php if ($totalRows_rsWOCKETS1 > 0) { // Show if recordset not empty ?>
            <?php do { ?>
              <tr class="<?php echo @$cnt1++%2==0 ? "" : "KT_even"; ?>">
                <td><input type="checkbox" name="kt_pk_WOCKETS" class="id_checkbox" value="<?php echo $row_rsWOCKETS1['id']; ?>" />
                    <input type="hidden" name="id" class="id_field" value="<?php echo $row_rsWOCKETS1['id']; ?>" />
                </td>
                <td><div class="KT_col_mac"><?php echo KT_FormatForList($row_rsWOCKETS1['mac'], 20); ?></div></td>
                <td><div class="KT_col_color"><?php echo KT_FormatForList($row_rsWOCKETS1['color'], 20); ?></div></td>
                <td><div class="KT_col_hardware"><?php echo KT_FormatForList($row_rsWOCKETS1['hardware'], 20); ?></div></td>
                <td><div class="KT_col_firmware"><?php echo KT_FormatForList($row_rsWOCKETS1['firmware'], 20); ?></div></td>
                <td><a class="KT_edit_link" href="WocketDetails.php?id=<?php echo $row_rsWOCKETS1['id']; ?>&amp;KT_back=1"><?php echo NXT_getResource("edit_one"); ?></a> <a class="KT_delete_link" href="#delete"><?php echo NXT_getResource("delete_one"); ?></a> </td>
              </tr>
              <?php } while ($row_rsWOCKETS1 = mysql_fetch_assoc($rsWOCKETS1)); ?>
            <?php } // Show if recordset not empty ?>
        </tbody>
      </table>
      <div class="KT_bottomnav">
        <div>
          <?php
            $nav_listWOCKETS1->Prepare();
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
        <a class="KT_additem_op_link" href="WocketDetails.php?KT_back=1" onclick="return nxt_list_additem(this)"><?php echo NXT_getResource("add new"); ?></a> </div>
    </form>
  </div>
  <br class="clearfixplain" />
</div>
<p>&nbsp;</p>
</body>
</html>
<?php
mysql_free_result($rsWOCKETS1);
?>
