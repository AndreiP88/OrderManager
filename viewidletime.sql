SELECT
  `ordersinprogress`.`count` AS `count`,
  `ordersinprogress`.`shiftID` AS `shiftID`,
  `ordersinprogress`.`machine` AS `machine`,
  `ordersinprogress`.`executor` AS `executor`,
  `ordersinprogress`.`typeJob` AS `typeJob`,
  `ordersinprogress`.`orderID` AS `orderID`,
  `ordersinprogress`.`makereadyConsider` AS `makereadyConsider`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`numberOfOrder`, 'Простой' ) AS `numberOfOrder`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`nameOfOrder`, `idletimelist`.`name` ) AS `nameOfOrder`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`modification`, '' ) AS `modification`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`amountOfOrder`, 0 ) AS `amountOfOrder`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`timeMakeready`, 0 ) AS `timeMakeready`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`timeToWork`, `idletime`.`normTime` ) AS `timeToWork`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `orders`.`statusOfOrder`, `idletime`.`status` ) AS `status`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`counterRepeat`, 0 ) AS `counterRepeat`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`timeMakereadyStart`, '' ) AS `timeMakereadyStart`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`timeMakereadyStop`, '' ) AS `timeMakereadyStop`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`timeToWorkStart`, `ordersinprogress`.`timeToWorkStart` ) AS `timeToWorkStart`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`timeToWorkStop`, `ordersinprogress`.`timeToWorkStop` ) AS `timeToWorkStop`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`done`, 0 ) AS `done`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`note`, `ordersinprogress`.`note` ) AS `note`,
IF
  ( ( `ordersinprogress`.`typeJob` = 0 ), `ordersinprogress`.`privateNote`, '' ) AS `privateNote`,
IF
  ( ( `ordersinprogress`.`typeJob` = 1 ), `idletime`.`checkIntoWorkingOut`,- ( 1 ) ) AS `checkIntoWorkingOut`,
IF
  ( ( `ordersinprogress`.`typeJob` = 1 ), `idletimelist`.`defaultNormTime`, 0 ) AS `defaultNormTime`,
IF
  ( ( `ordersinprogress`.`typeJob` = 1 ), `idletimelist`.`defaultCheckIntoWorkingOut`,- ( 1 ) ) AS `defaultCheckIntoWorkingOut`,
IF
  ( ( `ordersinprogress`.`typeJob` = 1 ), `idletimelist`.`idASystemIdletime`,- ( 1 ) ) AS `idASystemIdletime` 
FROM
  (
    (
      ( `orders` JOIN `ordersinprogress` ON ( ( `orders`.`count` = `ordersinprogress`.`orderID` ) ) )
      LEFT JOIN `idletime` ON ( ( `ordersinprogress`.`orderID` = `idletime`.`id` ) ) 
    )
    LEFT JOIN `idletimelist` ON ( ( `idletime`.`idIdletimeList` = `idletimelist`.`id` ) ) 
  )



















  SELECT
  `idletime`.`id` AS `id`,
  `idletime`.`machine` AS `machine`,
  `idletimelist`.`name` AS `name`,
  `idletime`.`normTime` AS `normTime`,
  `idletime`.`checkIntoWorkingOut` AS `checkIntoWorkingOut`,
  `idletime`.`status` AS `status`,
  `idletimelist`.`defaultNormTime` AS `defaultNormTime`,
  `idletimelist`.`defaultCheckIntoWorkingOut` AS `defaultCheckIntoWorkingOut` 
FROM
  ( `idletime` LEFT JOIN `idletimelist` ON ( ( `idletime`.`idIdletimeList` = `idletimelist`.`id` ) ) )