source("F:/Education/Party.r") #hjælpefunktioner (ie partyToFile)

#read data
Inside <- read.delim("F:/Education/insideFinish.txt") 
#Inside = Inside[sample(1:nrow(Inside), 1000),] #sample of data

#summary(Inside)
newrow = c(0,15,0,0,0,5,0,0,0) #ig=0, dur=0, hf=5
Inside = rbind(Inside,newrow)
newrow = c(0,15,0,5,0,12,0,0,0) #ig=5, hf=12
Inside = rbind(Inside,newrow)
newrow = c(0,15,0,1,0,0,0,0,0) #ig=1
Inside = rbind(Inside,newrow)
newrow = c(0,15,0,11,0,0,0,0,0) #ig=11
Inside = rbind(Inside,newrow)

Inside$alder = ordered(Inside$alder)
Inside$oprindelse = as.factor(Inside$oprindelse)
Inside$koen = ordered(Inside$koen)
Inside$ig = ordered(Inside$ig)
Inside$finish = ordered(Inside$finish)
Inside$dur = ordered(Inside$dur)
Inside$hf = ordered(Inside$hf)
Inside$n = as.integer(Inside$n)

#make tree
ctrl = partykit::ctree_control(minsplit=20L, minbucket = 7L)
tree <- partykit::ctree(finish~alder+oprindelse+koen+ig+dur+hf,data=Inside,weights=n,control=ctrl) #build tree

partyToFile(tree,"F:/Education/finishAlter.ctree") #print/export to file
