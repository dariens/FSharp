# -*- coding: utf-8 -*-
"""
Created on Tue Jan 17 08:47:06 2017

@author: darien.shannon
"""

import os
import sys
import pandas as pd
import numpy as np
import datetime


scriptLocation = os.path.dirname(sys.argv[0])

dataPath = os.path.dirname(__file__) + r'/Data'
#dataPath = 'U:/CODE/F#/Expert-FSharp-4.0/SalesByCustomer/Data'

joistSold = pd.read_csv(dataPath + '/Fallon/Jobs Sold.csv', encoding = "ISO-8859-1")
joistQuoted = pd.read_csv(dataPath + '/Fallon/Job Quoted.csv', encoding = "ISO-8859-1")
quotedVsSold = pd.read_csv(dataPath + '/Fallon/Quoted VS Sold.csv', encoding = "ISO-8859-1")

startDate = pd.to_datetime('2016-1-1')
endDate = pd.to_datetime('2016-12-31')

joistSold['J. PO Date'] = pd.to_datetime(pd.Series(joistSold['J. PO Date']))
quotedVsSold['Date'] = pd.to_datetime(pd.Series(quotedVsSold['Job Last Changed']))

filteredJoistSold = joistSold[(joistSold['J. PO Date'] >= startDate) & (joistSold['J. PO Date'] <= endDate)].replace(np.nan, 'BLANK', regex = True)

quotedVsSold = quotedVsSold[(quotedVsSold['Date'] >= startDate) & (quotedVsSold['Date'] <= endDate)].replace(np.nan, 'BLANK', regex = True)
quotedVsSold = quotedVsSold[['Customer','Job Number', 'J.Tons Sold', 'J. Sold Amnt']]
quotedVsSold = pd.DataFrame.drop_duplicates(quotedVsSold)

joistQuoted = joistQuoted[['Job Number', 'Total Tons' ]]

test = pd.merge(quotedVsSold, joistQuoted, on = 'Job Number')


joistSoldPODateFilt = filteredJoistSold[['Job Number', 'J. PO Date']]
joistSoldPODateFilt.columns = ['Job Number', 'PO Date']

soldPODateFilt = pd.concat([joistSoldPODateFilt])
soldPODateFilt = soldPODateFilt.drop_duplicates(subset = 'Job Number')

quotedVsSoldFilt = pd.merge(soldPODateFilt, quotedVsSold, on = 'Job Number')

filteredSold = pd.concat([filteredJoistSold]).replace(np.nan, 0, regex = True)
dsms = filteredSold['salesperson'].unique()

workbookName = '2016 Sales By Customer_' + '{:%Y-%m-%d @ %H;%M;%S}'.format(datetime.datetime.now()) 
writer = pd.ExcelWriter(workbookName + '.xlsx', engine='xlsxwriter')

workbook = writer.book
tonsFormat = workbook.add_format({'num_format': '#,##0.00'})
currencyFormat = workbook.add_format({'num_format': '$#,##0.00'})
###

test_temp = test.groupby(['Customer']).sum()
test_temp['Customer'] = test_temp.index


filteredSold_temp = quotedVsSoldFilt[['Customer', 'J.Tons Sold', 'J. Sold Amnt']].dropna()
filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
filteredSold_temp['Customer'] = filteredSold_temp.index

filteredSold_temp = pd.merge(filteredSold_temp, test_temp, on = 'Customer')

final = filteredSold_temp[['J.Tons Sold_x', 'J. Sold Amnt_x', 'Total Tons']]
final.index = filteredSold_temp['Customer']

final.to_excel(writer, 'ALL')
worksheet = writer.sheets['ALL']
worksheet.set_column('B:B', None, tonsFormat)
worksheet.set_column('C:C', None, currencyFormat)
worksheet.set_column('D:D', None, tonsFormat)
worksheet.set_column('E:E', None, currencyFormat)
worksheet.write('A1', 'Customer')
worksheet.write('B1', 'Joist Tons Sold')
worksheet.write('C1', 'Joist Sold Amnt')
worksheet.write('D1', 'Tons Quoted')
worksheet.add_table('A1:D' + str(len(filteredSold_temp.index)+1))




for dsm in dsms:
    filteredSold_temp = filteredSold.where(filteredSold['salesperson']==dsm)[['Customer', 'Total Tons', 'Plant Price']].dropna()
    filteredSold_temp = filteredSold_temp.groupby(['Customer']).sum()
    filteredSold_temp.to_excel(writer, dsm)
    worksheet.write('A1', 'Customer')
    worksheet.write('B1', 'Joist Tons')
    worksheet.write('C1', 'Joist Plant Price (W/ Bump)')
    worksheet = writer.sheets[dsm]
    worksheet.set_column('B:B', None, tonsFormat)
    worksheet.set_column('C:C', None, currencyFormat)
    worksheet.add_table('A1:C' + str(len(filteredSold_temp.index)+1))
    
    #p = Bar(filteredSold_temp, values = 'Plant Price', legend = False)
    #dsmFilename = dsm + '_' + str(startDate.date()) + '_TO_' + str(endDate.date())
    #output_file(scriptLocation + r'/output/' + dsmFilename + '.html')
    #show(p)
    
writer.save()

os.rename(scriptLocation + r'/' + workbookName + '.xlsx', scriptLocation + r'/Output/' + workbookName + '.xlsx')
