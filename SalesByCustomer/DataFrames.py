# -*- coding: utf-8 -*-
"""
Created on Tue Jan 17 08:47:06 2017

@author: darien.shannon
"""

import os
import sys
import pandas as pd
import numpy as np
#from bokeh.charts import Bar, output_file, show


scriptLocation = os.path.dirname(sys.argv[0])

dataPath = os.path.dirname(__file__) + r'/Data'

joistSold = pd.read_csv(dataPath + '/Fallon/Jobs Sold.csv', encoding = "ISO-8859-1")
deckSold = pd.read_csv(dataPath + '/Fallon/Deck Sold.csv', encoding = "ISO-8859-1")
quotedVsSold = pd.read_csv(dataPath + '/Fallon/Quoted VS Sold.csv', encoding = "ISO-8859-1")

startDate = pd.to_datetime('2016-1-1')
endDate = pd.to_datetime('2016-12-31')

joistSold['J. PO Date'] = pd.to_datetime(pd.Series(joistSold['J. PO Date']))
deckSold['PO Date'] = pd.to_datetime(pd.Series(deckSold['PO Date']))



filteredJoistSold = joistSold[(joistSold['J. PO Date'] >= startDate) & (joistSold['J. PO Date'] <= endDate)].replace(np.nan, 'BLANK', regex = True)
filteredDeckSold = deckSold[(deckSold['PO Date'] >= startDate) & (deckSold['PO Date'] <= endDate)].replace(np.nan, 'BLANK', regex = True)
filteredDeckSold['Deck Price'] = filteredDeckSold['Squares'] * filteredDeckSold['Unit Price']

joistSoldPODateFilt = filteredJoistSold[['Job Number', 'J. PO Date']]
joistSoldPODateFilt.columns = ['Job Number', 'PO Date']
deckSoldPODateFilt = filteredDeckSold[['Job Number', 'PO Date']]

soldPODateFilt = pd.concat([joistSoldPODateFilt, deckSoldPODateFilt])
soldPODateFilt = soldPODateFilt.drop_duplicates(subset = 'Job Number')

quotedVsSoldFilt = pd.merge(soldPODateFilt, quotedVsSold, on = 'Job Number')

filteredSold = pd.concat([filteredJoistSold, filteredDeckSold]).replace(np.nan, 0, regex = True)
dsms = filteredSold['salesperson'].unique()

workbookName = 'Workbook' + '_' + str(startDate.date()) + '_TO_' + str(endDate.date())
writer = pd.ExcelWriter(workbookName + '.xlsx', engine='xlsxwriter')

workbook = writer.book
tonsFormat = workbook.add_format({'num_format': '#,##0.00'})
currencyFormat = workbook.add_format({'num_format': '$#,##0.00'})
###

filteredSold_temp = quotedVsSoldFilt[['Customer', 'J.Tons Sold', 'J. Sold Amnt', 'D.Tons Sold', 'D. Sold Amnt']].dropna()
filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
filteredSold_temp.to_excel(writer, 'ALL')
worksheet = writer.sheets['ALL']
worksheet.set_column('B:B', None, tonsFormat)
worksheet.set_column('C:C', None, currencyFormat)
worksheet.set_column('D:D', None, tonsFormat)
worksheet.set_column('E:E', None, currencyFormat)
worksheet.add_table('A1:E' + str(len(filteredSold_temp.index)+1))
worksheet.write('A1', 'Customer')
worksheet.write('B1', 'Joist Tons Sold')
worksheet.write('C1', 'Joist Sold Amnt')
worksheet.write('D1', 'Deck Sold Tons')
worksheet.write('E1', 'Deck Sold Amnt')


###
filteredSold_temp = filteredSold[['Customer', 'Total Tons', 'Plant Price', 'Tons', 'Deck Price']].dropna()
filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
filteredSold_temp.to_excel(writer, 'ALL')
worksheet = writer.sheets['ALL']
worksheet.set_column('B:B', None, tonsFormat)
worksheet.set_column('C:C', None, currencyFormat)
worksheet.set_column('D:D', None, tonsFormat)
worksheet.set_column('E:E', None, currencyFormat)
worksheet.add_table('A1:E' + str(len(filteredSold_temp.index)+1))
worksheet.write('A1', 'Customer')
worksheet.write('B1', 'Joist Tons')
worksheet.write('C1', 'Joist Plant Price (W/ Bump)')
worksheet.write('D1', 'Deck Tons')
worksheet.write('E1', 'Deck Price (W/ Shipping)')


for dsm in dsms:
    filteredSold_temp = filteredSold.where(filteredSold['salesperson']==dsm)[['Customer', 'Total Tons', 'Plant Price', 'Tons', 'Deck Price']].dropna()
    filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
    filteredSold_temp.to_excel(writer, dsm)
    worksheet = writer.sheets[dsm]
    worksheet.set_column('B:B', None, tonsFormat)
    worksheet.set_column('C:C', None, currencyFormat)
    worksheet.set_column('D:D', None, tonsFormat)
    worksheet.set_column('E:E', None, currencyFormat)
    worksheet.add_table('A1:E' + str(len(filteredSold_temp.index)+1))
    worksheet.write('A1', 'Customer')
    worksheet.write('B1', 'Joist Tons')
    worksheet.write('C1', 'Joist Plant Price (W/ Bump)')
    worksheet.write('D1', 'Deck Tons')
    worksheet.write('E1', 'Deck Price (W/ Shipping)')
    #p = Bar(filteredSold_temp, values = 'Plant Price', legend = False)
    #dsmFilename = dsm + '_' + str(startDate.date()) + '_TO_' + str(endDate.date())
    #output_file(scriptLocation + r'/output/' + dsmFilename + '.html')
    #show(p)
    
#writer.save()

os.rename(scriptLocation + r'/' + workbookName + '.xlsx', scriptLocation + r'/Output/' + workbookName + '.xlsx')
