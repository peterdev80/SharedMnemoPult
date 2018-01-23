// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace fmslapi.VDL
{
    /// <summary>
    /// Коды инструкций исполняемой среды VDL
    /// </summary>
    internal enum OpCodes
    {
        Undefined, NOP,
        ADD, SUB, MUL, DIV, MATCH, EQU, NEQU, LSS, LSS_Z,
        GTR, GTR_Z, LSS_EQU, LSS_EQU_Z, GTR_EQU, GTR_EQU_Z, NEG, RMN,
        EQU_I32_Z, NEQU_I32_Z, EQU_F_Z, NEQU_F_Z, EQU_FALSE, EQU_TRUE, NEQU_FALSE, NEQU_TRUE,
        AND, OR, XOR, NOT, DUP, POP, SWAP,
        CALL_8, CALL_16,
        LOAD, LOADG, STO, STOG, CONSTI32, CONSTI32_SHORT, CONSTF, CONSTTRUE, CONSTFALSE,
        CONST_UNSET,
        CONSTI32_0, CONSTI32_1, CONST_NULL, CONST_F0, CONST_F1,
        PROPGET, PROPSET, CONVERTTO,
        RET, ENTER, LEAVE, JMP, FJMP, READ, WRITE,
        FJMP_P, TJMP_P,
        FORMAT, EXIT, CONSTSTRING8, CONSTSTRING16,
        LOAD_S, STO_S,
        LOADG0, LOADG1, LOADG2, LOADG3, LOADG4,
        LOAD0, LOAD1, LOAD2, LOAD3, LOAD4,
        STOG0, STOG1, STOG2, STOG3, STOG4,
        STOG_P, STOG0_P, STOG1_P, STOG2_P, STOG3_P, STOG4_P,
        STO0, STO1, STO2, STO3, STO4,
        STO_P, STO0_P, STO1_P, STO2_P, STO3_P, STO4_P,
        GETCONF,
        STATREF,
        CONSTD, CONST_D0, CONST_D1, CONSTD_PI, CONSTD_2PI,
        SIN, COS, TAN, ASIN, ACOS, ATAN,
        LSHIFT, RSHIFT,
        LOCALIZE,
        MATCH_OPT
    }
}
