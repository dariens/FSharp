# -*- coding: utf-8 -*-
"""
Created on Tue Jan 17 08:47:06 2017

@author: darien.shannon
"""

import os
import sys
import pandas as pd


scriptLocation = os.path.dirname(sys.argv[0])

dataPath = os.path.dirname(__file__) + r'/Data'

quotedVsSold = pd.read_csv(dataPath + '/Fallon/Quoted VS Sold.csv', encoding = "ISO-8859-1")
quotedVsSold['From Customer Service'] = [any(c.isalpha() for c in jobNumber.split('-')[1]) for jobNumber in quotedVsSold['Job Number']]
quotedVsSold = quotedVsSold[quotedVsSold['From Customer Service'] == False]

startDate = pd.to_datetime('2016-1-1')
endDate = pd.to_datetime('2016-12-31')

workbookName = 'Workbook' + '_' + str(startDate.date()) + '_TO_' + str(endDate.date())
writer = pd.ExcelWriter(workbookName + '.xlsx', engine='xlsxwriter')

workbook = writer.book
tonsFormat = workbook.add_format({'num_format': '#,##0.00'})
currencyFormat = workbook.add_format({'num_format': '$#,##0.00'})

###

filteredSold_temp = quotedVsSold[['Customer', 'J.Tons Sold', 'J. Sold Amnt', 'D.Tons Sold', 'D. Sold Amnt']]
filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
filteredSold_temp.to_excel(writer, 'ALL')
worksheet = writer.sheets['ALL']
worksheet.set_column('B:B', None, tonsFormat)
worksheet.set_column('C:C', None, currencyFormat)
worksheet.set_column('D:D', None, tonsFormat)
worksheet.set_column('E:E', None, currencyFormat)
worksheet.write('A1', 'Customer')
worksheet.write('B1', 'Joist Tons Sold')
worksheet.write('C1', 'Joist Sold Amnt')
worksheet.write('D1', 'Deck Sold Tons')
worksheet.write('E1', 'Deck Sold Amnt')

###

  
writer.save()

os.rename(scriptLocation + r'/' + workbookName + '.xlsx', scriptLocation + r'/Output/' + workbookName + '.xlsx')