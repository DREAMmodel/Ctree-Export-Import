library(partykit)
library(Formula)

## Makes party-object for CTREE
nodeCount <- 0
partyToFile_split = function(node, hFile, pr)
{
  
  split = node$split
  kids  = node$kids
  index = split$index
  breaks = split$breaks
  
  cat("node$id   \t", node$id,              "\n", file = hFile, sep = "")
  cat("is.terminal  \t", is.terminal(node), "\n", file = hFile, sep = "")
  
  if (!is.terminal(node)) 
  {
    cat("split$varid    \t", split$varid,                 "\n", file = hFile, sep = "")
    cat("split$index    \t", paste(index, collapse="\t"), "\n", file = hFile, sep = "")
    cat("split$breaks   \t", paste(breaks, collapse="\t"),"\n", file = hFile, sep = "")  
  }
  else
  {
    nodeCount <<- nodeCount + 1
    cat("probabilities \t", paste(pr[nodeCount, ], collapse="\t"), "\n", file = hFile, sep = "")    
  }
  
  cat("============================",                "\n", file = hFile, sep = "")
  
  if (length(kids) > 0)
    for(i in 1:length(kids))
      partyToFile_split(kids[[i]], hFile, pr)
  
}

partyToFile = function(p, fileName)
{
  
  if(!is(p, "party"))
    return("Error in partyToFile: Not party object")
  
  print("Calculating probabilities...", quote=F)
  pr = GetProbabilities(p)    
  
  print("Writing to file...", quote=F )
  hFile = file(fileName, "w")
  
  nodeCount <<- 0
  
  data = p$data
  
  # Number of variables
  cat("Rank        \t", length(data[1, ]) - 1, "\n", file = hFile, sep = "")
  
  cat("length(Y",  ")\t", length(levels(data[ ,1])), "\n", file = hFile, sep = "")
  
  for(i in 2:length(data[1, ]))
    cat("length(X", i-2,  ")\t", length(levels(data[ ,i])), "\n", file = hFile, sep = "")
  cat("============================",                "\n", file = hFile, sep = "")
  
  names = names(data)
  
  cat("Y  ",  "\t", names[1], "\n", file = hFile, sep = "")
  for(i in 2:length(data[1, ]))
    cat("X", i-2,  "\t", names[i], "\n", file = hFile, sep = "")
  cat("============================",                "\n", file = hFile, sep = "")

  cat("is.ordered(Y)",  "\t", is.ordered(data[ ,1]), "\n", file = hFile, sep = "")
  for(i in 2:length(data[1, ]))
    cat("is.ordered(X", i-2,  ")\t", is.ordered(data[ ,i]), "\n", file = hFile, sep = "")
  cat("============================",                "\n", file = hFile, sep = "")
  
  
  cat("levels(Y)",  "\t", paste(levels(data[ ,1]),collapse="\t"), "\n", file = hFile, sep = "")
  
  for(i in 2:length(data[1, ]))
    cat("levels(X", i-2,  ")\t", paste(levels(data[ ,i]),collapse="\t"), "\n", file = hFile, sep = "")
  cat("============================",                "\n", file = hFile, sep = "")
  
  
  # Start the recursive fun!
  partyToFile_split(p$node, hFile, pr)
  
  close(hFile)
  print("Done.", quote=F )
  
}


## Calculates probabilities using the ctree - Part of the party-object
# Input:     party-object
# includeID: If true, include terminal node id's in data.frame
# Output:    data.frame, containing all terminal nodes. Each line contains 
#            probabilities (two or more) and node number.
GetProbabilities = function(partyObj, includeID=FALSE)
{
  term_nodes = nodeids(partyObj, terminal = T)
  pr = as.data.frame(predict(partyObj, id = term_nodes, type = "prob", simplify = T))
  if(includeID) pr$id = term_nodes
  return(pr)
}


# Misc help-functions
calcProb = function(d)
{
  
  f = as.numeric(table(d))  
  f = f / sum(f)
  return(list(f))
  
}

calcNum = function(d)
{
  f = as.numeric(table(d))  
  return(list(f))
}


calcProbPlus = function(d)
{
  
  f = as.numeric(table(d))  
  sf = sum(f)
  f = f / sf
  length(f) = length(f) + 1
  f[length(f)] = sf
  return(list(f))
  
}


formulaToInfoFile = function(formula, filename)
{
  hFile = file(filename, "w")
  sVar = attr(terms(formula),"variables")
  cat("Y\t",sVar[[2]],"\n", file = hFile, sep = "")
  for(i in 3:length(sVar))
    cat("X",i-3, "\t", sVar[[i]],"\n", file = hFile, sep = "")
  close(hFile)
  
}


