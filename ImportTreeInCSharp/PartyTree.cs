using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Dream.R
{

  #region TestObject
  /// <summary>
  /// Bruges til test af PartyTree. Opsamler informationer fra det enkelte gennemløb af indexeren
  /// </summary>
  public class TestObject<T>
  {
    #region Public field
    /// <summary>
    /// Angiver de elementer der ikke er mulig
    /// </summary>
    public bool[][] NotPossible;
    #endregion

    #region Private fields
    /// <summary>
    /// Debth in tree
    /// </summary>
    public int Debth;
    PartyTree<T> _tree;
    #endregion

    #region Constructors
    public TestObject(PartyTree<T> tree)
    {
      Debth = 0;
      _tree = tree;

      NotPossible = new bool[_tree.Rank][];
      for (int i = 0; i < _tree.Rank; i++)
        NotPossible[i] = new bool[_tree.GetLength(i)];
    }
    #endregion

    #region Initialize
    public void Initialize()
    {
      Debth = 0;
    }
    #endregion

  }
  #endregion

  #region Class PartyTree
  public class PartyTree<T>
  {
    public const bool REPORTCONSOLE = false;

    #region Public fields
    public int NAs = 0; // Counts the number of times the indexer returns -1
    // Testing
    /// <summary>
    /// Hvis true køres test
    /// </summary>
    public bool Test = false;
    /// <summary>
    /// Optæller matrice fordelt på varId og Debth
    /// </summary>
    public int[,] TCount;
    /// <summary>
    /// Optæller fordelt på nodeId
    /// </summary>
    public int[] TCountID;
    /// <summary>
    /// Optæller fordelt på nodeId (eksl. terminalnodes)
    /// </summary>
    public int[] TCountID_NoTerm;
    /// <summary>
    /// Optæller fordelt på nodeId (terminalnodes)
    /// </summary>
    public int[] TCountID_Term;
    /// <summary>
    /// Optæller matrice fordelt på varId og break-element
    /// </summary>
    public int[,] TCountBreak;
    /// <summary>
    /// Optæller antal split fordelt på varId, debth og break-element
    /// </summary>
    public int[, ,] TCountBreakDebth;
    /// <summary>
    /// Træest maksimale dybte
    /// </summary>
    public int TMaxDepth = 50;
    /// <summary>
    /// Maksimalt antal nodes
    /// </summary>
    public int TMaxNodeId = 50000;
    #endregion

    #region Private fields
    public PartyNode<T> _node;  // Root node
    int _requests;
    int _rank; // Number of dimensions
    int[] _lengthX, _baseX;
    int _lengthY;
    string _nameY;
    string[] _nameX;
    bool _isOrderedY;
    bool[] _isOrderedX;
    string[] _levelsY;
    string[][] _levelsX;
    StreamReader _stream;
    #endregion

    #region Constructors
    public PartyTree(string fileName)
      : this(fileName, false)
    {
    }

    public PartyTree(string fileName, bool test)
    {
      Test = test;

      _stream = new StreamReader(fileName);

      _rank = Convert.ToInt32(_stream.ReadLine().Split('\t')[1]); //number of variables
      _lengthY = Convert.ToInt32(_stream.ReadLine().Split('\t')[1]);
      _lengthX = new int[_rank];
      _nameX = new string[_rank];
      _isOrderedX = new bool[_rank];
      _levelsX = new string[_rank][];
      _baseX = new int[_rank];

      for (int i = 0; i < _rank; i++) //find out level size
        _lengthX[i] = Convert.ToInt32(_stream.ReadLine().Split('\t')[1]);

      _stream.ReadLine(); // Read "============================"

      _nameY = _stream.ReadLine().Split('\t')[1];

      for (int i = 0; i < _rank; i++)
        _nameX[i] = _stream.ReadLine().Split('\t')[1];

      _stream.ReadLine(); // Read "============================"

      _isOrderedY = Convert.ToBoolean(_stream.ReadLine().Split('\t')[1]);

      for (int i = 0; i < _rank; i++)
        _isOrderedX[i] = Convert.ToBoolean(_stream.ReadLine().Split('\t')[1]);

      _stream.ReadLine(); // Read "============================"

      _levelsY = new string[_lengthY];
      string[] s = _stream.ReadLine().Split('\t');
      for (int i = 0; i < _lengthY; i++)
        _levelsY[i] = s[1 + i];


      for (int i = 0; i < _rank; i++)
      {
        _levelsX[i] = new string[_lengthX[i]];
        s = _stream.ReadLine().Split('\t');
        for (int j = 0; j < _lengthX[i]; j++)
          _levelsX[i][j] = s[1 + j];

        if (_isOrderedX[i])
          _baseX[i] = Convert.ToInt32(_levelsX[i][0]);
      }

      _stream.ReadLine(); // Read "============================"

      if (Test)
      {
        this.TCount = new int[_rank, TMaxDepth];
        this.TCountID = new int[TMaxNodeId];
        this.TCountID_NoTerm = new int[TMaxNodeId];
        this.TCountID_Term = new int[TMaxNodeId];
        this.TCountBreak = new int[_rank, _lengthX.Max()];
        this.TCountBreakDebth = new int[_rank, _lengthX.Max(), TMaxDepth];
      }

      _node = new PartyNode<T>(this, null); // Start recursive reading of tree

      _stream.Close();
    }
    #endregion

    #region Indexer
    ///<summary>
    /// C#-indexer. Can be called DREAMNode[int,int...int]. Finds corresponding terminal node by looking through tree
    ///</summary>
    public T[] this[params int[] var]
    {
      get
      {
        if (var.Length != _rank)
          throw new Exception(String.Format("Wrong number of indicies inside [] in PartyTree; expected {0}", _rank));

        _requests++; //count number of requests

        if (Test)
          _node.TestObject.Initialize();

        PartyNode<T> parent = _node, child;
        while ((child = parent[var]) != null) // Forsæt indtil terminal-node er fundet
          parent = child; // kør længere ned i træet
        return parent.Value; // Hvis child == null har vi nået terminal-node og resultat returneres
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets a 32-bit integer that represents the number of elements in the specified dimension of the PartyTree
    /// </summary>
    /// <param name="i">A zero-based dimension</param>
    /// <returns>Int32</returns>
    public int GetLength(int i)
    {
      return _lengthX[i];
    }

    ///<summary>
    /// Get number of times whole tree has been requested (can also be used to benchmark inner nodes)
    ///</summary>
    public int getRequests()
    {
      return _requests;
    }
    #endregion

    #region Proporties
    /// <summary>
    /// Gets the rank (number of dimensions)
    /// </summary>
    public int Rank
    {
      get { return _rank; }
    }

    public int LengthX(int j)
    {
      return _lengthX[j];
    }


    // Bør implementeres som LengthX (brugeren kan skrive i array)
    public int[] BaseX
    {
      get { return _baseX; }
    }
    public bool IsOrderedX(int j)
    {
      return _isOrderedX[j];
    }

    public int LengthY
    {
      get { return _lengthY; }
    }
    public StreamReader Stream
    {
      get { return _stream; }
    }
    public int Requests
    {
      get { return _requests; }
    }
    #endregion

  }
  #endregion

  #region Class PartyNode
  public class PartyNode<T>
  {

    #region Private fields
    ///<summary>
    /// Value of the node. Is empty if the node is not a terminal node
    ///</summary>
    T[] _val;
    ///<summary>
    /// An array to hold references to the children of the node. Set to null if terminal node.
    ///</summary>
    PartyNode<T>[] _children;
    ///<summary>
    /// ID of the node given by input file. Mostly used for debugging.
    ///</summary>
    int _nodeId;
    ///<summary>
    /// Is the number of the variable the node splits its childrens by
    ///</summary>
    int _varId;

    PartyTree<T> _tree;
    int[] _index;
    int[] _breaks;

    TestObject<T> _testObject;
    PartyNode<T> _parent;
    #endregion

    #region Constructors
    public PartyNode(PartyTree<T> tree, PartyNode<T> parent)
    {
      _tree = tree;
      _parent = parent;

      if (_tree.Test)
      {
        _testObject = new TestObject<T>(_tree);

        if (_parent != null)
          _testObject.Debth = _parent.TestObject.Debth + 1;
        else
          _testObject.Debth = 0;
      }

      StreamReader streamStruct = _tree.Stream;

      _nodeId = Convert.ToInt32(streamStruct.ReadLine().Split('\t')[1]); //get node id

      bool isTerminalNode = streamStruct.ReadLine().Split('\t')[1] == "TRUE";

      if (isTerminalNode)
      {
        string[] probabilitiesTmp = streamStruct.ReadLine().Split('\t');
        _val = new T[_tree.LengthY]; //beskriver sandsynlighedsfordelingen givet i en endnode

        for (int i = 1; i < probabilitiesTmp.Length; i++)
          _val[i - 1] = (T)Convert.ChangeType(probabilitiesTmp[i], typeof(T));

        streamStruct.ReadLine(); // Read line with ======================       
        return;
      }
      else // If not terminal node
      {
        _varId = Convert.ToInt32(streamStruct.ReadLine().Split('\t')[1]) - 2; // C# er 0-baseret, R er 1-baseret + first variable is Y: therefore - 2

        string[] indexParts = streamStruct.ReadLine().Split('\t'); //numbered list of children
        bool isIndex = indexParts[1] != "";   // Index or breaks?

        if (isIndex)
        {
          streamStruct.ReadLine(); // Read empty breaks line
          streamStruct.ReadLine(); // Read line with ======================

          _index = new int[tree.GetLength(_varId)];

          // Just for testing
          if (indexParts.Length - 1 != tree.GetLength(_varId))
            throw new Exception("Ups!!");

          // Read _index
          for (int i = 1; i < indexParts.Length; i++) //tæller fra 1 fordi 0 er "split$index"
          {
            if (indexParts[i] == "NA") //der findes intet split for denne gruppe
            {
              /// HACK !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
              /// Meget alvorligt hack: overskriver NA 
              /// 
              /// NER: Hvis en gruppe ikke findes ved split på index gives denne gruppe sandsynlighed som gruppen forinden
              ///      dvs. hvis grupperne er: dk, vestlige-inv., ikke-vestlige-indvand, vestlige-efterkommere, ikke-vestlige-efterkommere.
              ///      Så vil vestlige-efterkommere få samme sandsynlighedsfordeling som ikke-vestlige-indvand såfremt der ikke findes vestlige-efterkommere ved noden.
              if (i == 1)
                _index[i - 1] = 0; //første element er NA, sæt til at være lig index 0
              else
                _index[i - 1] = _index[i - 2]; //index for elementet sættes til værdien af foregående split
            }
            else
              _index[i - 1] = Convert.ToInt32(indexParts[i]) - 1; // C# er 0-baseret, R er 1-baseret
          }

          // Calculate nChildren
          int nChildren = 0;
          for (int i = 0; i < _index.Length; i++)
            if (_index[i] >= nChildren) nChildren = _index[i];

          nChildren++; // C# er 0-baseret, R er 1-baseret

          _children = new PartyNode<T>[nChildren]; // Make children (not war)

          for (int i = 0; i < nChildren; i++)
            _children[i] = new PartyNode<T>(tree, this);

          return;
        }
        else  // If breaks
        {
          string[] sBreaks = streamStruct.ReadLine().Split('\t');
          streamStruct.ReadLine(); // Read line with ======================          

          _breaks = new int[sBreaks.Length - 1];

          for (int i = 1; i < sBreaks.Length; i++)
          {
            _breaks[i - 1] = Convert.ToInt32(sBreaks[i]);
            if (_breaks[i - 1] < 0)
              throw new Exception("_breaks[i - 1] < 0");
          }

          _children = new PartyNode<T>[_breaks.Length + 1]; // Make children (not war)

          for (int i = 0; i < _breaks.Length + 1; i++)
            _children[i] = new PartyNode<T>(tree, this);

          return;
        }
      }
    }
    #endregion

    #region Indexer
    ///<summary>
    /// C#-indexer. Can be called DREAMNode[int,int...int]. Finds corresponding terminal node by looking through tree
    ///</summary>
    public PartyNode<T> this[params int[] var]
    {
      get
      {

        #region _tree.Test
        if (_tree.Test)
        {
          //if (_parent != null)
          //  _testObject.Debth = _parent.TestObject.Debth + 1;
          //else
          //  _testObject.Debth = 0;

          if (_children != null) // Hvis ikke terminal-node
            if (_testObject.Debth < _tree.TMaxDepth)
            {
              _tree.TCount[_varId, _testObject.Debth]++;
              _tree.TCountID_NoTerm[_nodeId]++;
            }

          if (_children == null) // Hvis terminal-node
          {
            _tree.TCountID_Term[_nodeId]++;
          }

          _tree.TCountID[_nodeId]++;

          if (_breaks != null)   // Hvis breaks
          {
            _tree.TCountBreak[_varId, _breaks[0]]++; // Antager der kun et 1 breaks !!!!!!!          

            if (_testObject.Debth < _tree.TMaxDepth)
              _tree.TCountBreakDebth[_varId, _breaks[0], _testObject.Debth]++; // Antager der kun et 1 breaks !!!!!!!
          }
        }
        #endregion

        if (PartyTree<T>.REPORTCONSOLE)
        {
          Console.WriteLine("==================================");
          Console.WriteLine("_nodeId = {0}", _nodeId);
        }

        if (_children == null)     // If terminal
          return null;
        else if (_breaks == null)  // If index
        {

          if (PartyTree<T>.REPORTCONSOLE) Console.WriteLine("Index:");
          if (PartyTree<T>.REPORTCONSOLE) Console.WriteLine("_varID = {0}", _varId);
          if (PartyTree<T>.REPORTCONSOLE) Console.WriteLine("_index[var[_varId]] = {0}", _index[var[_varId]]);

          int index = _index[var[_varId]];

          if (index == -1)  // I denne situation: opførsel som terminal-node. Returnerer ssh = -1
          {
            _tree.NAs++;
            _val = new T[_tree.LengthY];
            for (int i = 0; i < _val.Length; i++)
              _val[i] = (T)Convert.ChangeType(-1.0, typeof(T));
            return null;
          }

          return _children[index];
        }
        else                       // If breaks
        {
          if (PartyTree<T>.REPORTCONSOLE)
          {
            Console.WriteLine("Breaks:");
            Console.WriteLine("_varID = {0}", _varId);
            Console.WriteLine("_breaks.Length = {0}", _breaks.Length);
            for (int i = 0; i < _breaks.Length; i++)
              Console.WriteLine("_break{0} = {1}", i + 1, _breaks[i]);
          }

          for (int i = 0; i < _breaks.Length; i++)
            if (var[_varId] < _breaks[i])// - _tree.BaseX[_varId])    // < as right = TRUE in party
              return _children[i];

          return _children[_children.Length - 1];
        }
      }
    }
    #endregion

    #region Proporties
    /// <summary>
    /// NER: Når værdien af træet returneres returneres en pointer til et array. Vær varsom med at ændre dette array. Hvis ikke der først oprettes en kopi af array'et, skrives der i træet, hvilket kan medfører alvorlige fejl, der er svære at finde!
    /// </summary>
    public T[] Value
    {
      get
      {
        return _val;
      }
    }

    public TestObject<T> TestObject
    {
      get { return _testObject; }
    }
    #endregion

  }
  #endregion
}