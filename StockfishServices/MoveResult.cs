﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockfishServices;

public record MoveResult(MoveEval Move, int LostCentPawns, MoveClassification Classification);
