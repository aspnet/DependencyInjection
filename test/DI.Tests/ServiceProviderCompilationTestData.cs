// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    interface I0 { }
    class S0 : I0 { }
    interface I1 { }
    class S1 : I1 { public S1(I0 s) { } }
    interface I2 { }
    class S2 : I2 { public S2(I1 s) { } }
    interface I3 { }
    class S3 : I3 { public S3(I2 s) { } }
    interface I4 { }
    class S4 : I4 { public S4(I3 s) { } }
    interface I5 { }
    class S5 : I5 { public S5(I4 s) { } }
    interface I6 { }
    class S6 : I6 { public S6(I5 s) { } }
    interface I7 { }
    class S7 : I7 { public S7(I6 s) { } }
    interface I8 { }
    class S8 : I8 { public S8(I7 s) { } }
    interface I9 { }
    class S9 : I9 { public S9(I8 s) { } }
    interface I10 { }
    class S10 : I10 { public S10(I9 s) { } }
    interface I11 { }
    class S11 : I11 { public S11(I10 s) { } }
    interface I12 { }
    class S12 : I12 { public S12(I11 s) { } }
    interface I13 { }
    class S13 : I13 { public S13(I12 s) { } }
    interface I14 { }
    class S14 : I14 { public S14(I13 s) { } }
    interface I15 { }
    class S15 : I15 { public S15(I14 s) { } }
    interface I16 { }
    class S16 : I16 { public S16(I15 s) { } }
    interface I17 { }
    class S17 : I17 { public S17(I16 s) { } }
    interface I18 { }
    class S18 : I18 { public S18(I17 s) { } }
    interface I19 { }
    class S19 : I19 { public S19(I18 s) { } }
    interface I20 { }
    class S20 : I20 { public S20(I19 s) { } }
    interface I21 { }
    class S21 : I21 { public S21(I20 s) { } }
    interface I22 { }
    class S22 : I22 { public S22(I21 s) { } }
    interface I23 { }
    class S23 : I23 { public S23(I22 s) { } }
    interface I24 { }
    class S24 : I24 { public S24(I23 s) { } }
    interface I25 { }
    class S25 : I25 { public S25(I24 s) { } }
    interface I26 { }
    class S26 : I26 { public S26(I25 s) { } }
    interface I27 { }
    class S27 : I27 { public S27(I26 s) { } }
    interface I28 { }
    class S28 : I28 { public S28(I27 s) { } }
    interface I29 { }
    class S29 : I29 { public S29(I28 s) { } }
    interface I30 { }
    class S30 : I30 { public S30(I29 s) { } }
    interface I31 { }
    class S31 : I31 { public S31(I30 s) { } }
    interface I32 { }
    class S32 : I32 { public S32(I31 s) { } }
    interface I33 { }
    class S33 : I33 { public S33(I32 s) { } }
    interface I34 { }
    class S34 : I34 { public S34(I33 s) { } }
    interface I35 { }
    class S35 : I35 { public S35(I34 s) { } }
    interface I36 { }
    class S36 : I36 { public S36(I35 s) { } }
    interface I37 { }
    class S37 : I37 { public S37(I36 s) { } }
    interface I38 { }
    class S38 : I38 { public S38(I37 s) { } }
    interface I39 { }
    class S39 : I39 { public S39(I38 s) { } }
    interface I40 { }
    class S40 : I40 { public S40(I39 s) { } }
    interface I41 { }
    class S41 : I41 { public S41(I40 s) { } }
    interface I42 { }
    class S42 : I42 { public S42(I41 s) { } }
    interface I43 { }
    class S43 : I43 { public S43(I42 s) { } }
    interface I44 { }
    class S44 : I44 { public S44(I43 s) { } }
    interface I45 { }
    class S45 : I45 { public S45(I44 s) { } }
    interface I46 { }
    class S46 : I46 { public S46(I45 s) { } }
    interface I47 { }
    class S47 : I47 { public S47(I46 s) { } }
    interface I48 { }
    class S48 : I48 { public S48(I47 s) { } }
    interface I49 { }
    class S49 : I49 { public S49(I48 s) { } }
    interface I50 { }
    class S50 : I50 { public S50(I49 s) { } }
    interface I51 { }
    class S51 : I51 { public S51(I50 s) { } }
    interface I52 { }
    class S52 : I52 { public S52(I51 s) { } }
    interface I53 { }
    class S53 : I53 { public S53(I52 s) { } }
    interface I54 { }
    class S54 : I54 { public S54(I53 s) { } }
    interface I55 { }
    class S55 : I55 { public S55(I54 s) { } }
    interface I56 { }
    class S56 : I56 { public S56(I55 s) { } }
    interface I57 { }
    class S57 : I57 { public S57(I56 s) { } }
    interface I58 { }
    class S58 : I58 { public S58(I57 s) { } }
    interface I59 { }
    class S59 : I59 { public S59(I58 s) { } }
    interface I60 { }
    class S60 : I60 { public S60(I59 s) { } }
    interface I61 { }
    class S61 : I61 { public S61(I60 s) { } }
    interface I62 { }
    class S62 : I62 { public S62(I61 s) { } }
    interface I63 { }
    class S63 : I63 { public S63(I62 s) { } }
    interface I64 { }
    class S64 : I64 { public S64(I63 s) { } }
    interface I65 { }
    class S65 : I65 { public S65(I64 s) { } }
    interface I66 { }
    class S66 : I66 { public S66(I65 s) { } }
    interface I67 { }
    class S67 : I67 { public S67(I66 s) { } }
    interface I68 { }
    class S68 : I68 { public S68(I67 s) { } }
    interface I69 { }
    class S69 : I69 { public S69(I68 s) { } }
    interface I70 { }
    class S70 : I70 { public S70(I69 s) { } }
    interface I71 { }
    class S71 : I71 { public S71(I70 s) { } }
    interface I72 { }
    class S72 : I72 { public S72(I71 s) { } }
    interface I73 { }
    class S73 : I73 { public S73(I72 s) { } }
    interface I74 { }
    class S74 : I74 { public S74(I73 s) { } }
    interface I75 { }
    class S75 : I75 { public S75(I74 s) { } }
    interface I76 { }
    class S76 : I76 { public S76(I75 s) { } }
    interface I77 { }
    class S77 : I77 { public S77(I76 s) { } }
    interface I78 { }
    class S78 : I78 { public S78(I77 s) { } }
    interface I79 { }
    class S79 : I79 { public S79(I78 s) { } }
    interface I80 { }
    class S80 : I80 { public S80(I79 s) { } }
    interface I81 { }
    class S81 : I81 { public S81(I80 s) { } }
    interface I82 { }
    class S82 : I82 { public S82(I81 s) { } }
    interface I83 { }
    class S83 : I83 { public S83(I82 s) { } }
    interface I84 { }
    class S84 : I84 { public S84(I83 s) { } }
    interface I85 { }
    class S85 : I85 { public S85(I84 s) { } }
    interface I86 { }
    class S86 : I86 { public S86(I85 s) { } }
    interface I87 { }
    class S87 : I87 { public S87(I86 s) { } }
    interface I88 { }
    class S88 : I88 { public S88(I87 s) { } }
    interface I89 { }
    class S89 : I89 { public S89(I88 s) { } }
    interface I90 { }
    class S90 : I90 { public S90(I89 s) { } }
    interface I91 { }
    class S91 : I91 { public S91(I90 s) { } }
    interface I92 { }
    class S92 : I92 { public S92(I91 s) { } }
    interface I93 { }
    class S93 : I93 { public S93(I92 s) { } }
    interface I94 { }
    class S94 : I94 { public S94(I93 s) { } }
    interface I95 { }
    class S95 : I95 { public S95(I94 s) { } }
    interface I96 { }
    class S96 : I96 { public S96(I95 s) { } }
    interface I97 { }
    class S97 : I97 { public S97(I96 s) { } }
    interface I98 { }
    class S98 : I98 { public S98(I97 s) { } }
    interface I99 { }
    class S99 : I99 { public S99(I98 s) { } }
    interface I100 { }
    class S100 : I100 { public S100(I99 s) { } }
    interface I101 { }
    class S101 : I101 { public S101(I100 s) { } }
    interface I102 { }
    class S102 : I102 { public S102(I101 s) { } }
    interface I103 { }
    class S103 : I103 { public S103(I102 s) { } }
    interface I104 { }
    class S104 : I104 { public S104(I103 s) { } }
    interface I105 { }
    class S105 : I105 { public S105(I104 s) { } }
    interface I106 { }
    class S106 : I106 { public S106(I105 s) { } }
    interface I107 { }
    class S107 : I107 { public S107(I106 s) { } }
    interface I108 { }
    class S108 : I108 { public S108(I107 s) { } }
    interface I109 { }
    class S109 : I109 { public S109(I108 s) { } }
    interface I110 { }
    class S110 : I110 { public S110(I109 s) { } }
    interface I111 { }
    class S111 : I111 { public S111(I110 s) { } }
    interface I112 { }
    class S112 : I112 { public S112(I111 s) { } }
    interface I113 { }
    class S113 : I113 { public S113(I112 s) { } }
    interface I114 { }
    class S114 : I114 { public S114(I113 s) { } }
    interface I115 { }
    class S115 : I115 { public S115(I114 s) { } }
    interface I116 { }
    class S116 : I116 { public S116(I115 s) { } }
    interface I117 { }
    class S117 : I117 { public S117(I116 s) { } }
    interface I118 { }
    class S118 : I118 { public S118(I117 s) { } }
    interface I119 { }
    class S119 : I119 { public S119(I118 s) { } }
    interface I120 { }
    class S120 : I120 { public S120(I119 s) { } }
    interface I121 { }
    class S121 : I121 { public S121(I120 s) { } }
    interface I122 { }
    class S122 : I122 { public S122(I121 s) { } }
    interface I123 { }
    class S123 : I123 { public S123(I122 s) { } }
    interface I124 { }
    class S124 : I124 { public S124(I123 s) { } }
    interface I125 { }
    class S125 : I125 { public S125(I124 s) { } }
    interface I126 { }
    class S126 : I126 { public S126(I125 s) { } }
    interface I127 { }
    class S127 : I127 { public S127(I126 s) { } }
    interface I128 { }
    class S128 : I128 { public S128(I127 s) { } }
    interface I129 { }
    class S129 : I129 { public S129(I128 s) { } }
    interface I130 { }
    class S130 : I130 { public S130(I129 s) { } }
    interface I131 { }
    class S131 : I131 { public S131(I130 s) { } }
    interface I132 { }
    class S132 : I132 { public S132(I131 s) { } }
    interface I133 { }
    class S133 : I133 { public S133(I132 s) { } }
    interface I134 { }
    class S134 : I134 { public S134(I133 s) { } }
    interface I135 { }
    class S135 : I135 { public S135(I134 s) { } }
    interface I136 { }
    class S136 : I136 { public S136(I135 s) { } }
    interface I137 { }
    class S137 : I137 { public S137(I136 s) { } }
    interface I138 { }
    class S138 : I138 { public S138(I137 s) { } }
    interface I139 { }
    class S139 : I139 { public S139(I138 s) { } }
    interface I140 { }
    class S140 : I140 { public S140(I139 s) { } }
    interface I141 { }
    class S141 : I141 { public S141(I140 s) { } }
    interface I142 { }
    class S142 : I142 { public S142(I141 s) { } }
    interface I143 { }
    class S143 : I143 { public S143(I142 s) { } }
    interface I144 { }
    class S144 : I144 { public S144(I143 s) { } }
    interface I145 { }
    class S145 : I145 { public S145(I144 s) { } }
    interface I146 { }
    class S146 : I146 { public S146(I145 s) { } }
    interface I147 { }
    class S147 : I147 { public S147(I146 s) { } }
    interface I148 { }
    class S148 : I148 { public S148(I147 s) { } }
    interface I149 { }
    class S149 : I149 { public S149(I148 s) { } }
    interface I150 { }
    class S150 : I150 { public S150(I149 s) { } }
    interface I151 { }
    class S151 : I151 { public S151(I150 s) { } }
    interface I152 { }
    class S152 : I152 { public S152(I151 s) { } }
    interface I153 { }
    class S153 : I153 { public S153(I152 s) { } }
    interface I154 { }
    class S154 : I154 { public S154(I153 s) { } }
    interface I155 { }
    class S155 : I155 { public S155(I154 s) { } }
    interface I156 { }
    class S156 : I156 { public S156(I155 s) { } }
    interface I157 { }
    class S157 : I157 { public S157(I156 s) { } }
    interface I158 { }
    class S158 : I158 { public S158(I157 s) { } }
    interface I159 { }
    class S159 : I159 { public S159(I158 s) { } }
    interface I160 { }
    class S160 : I160 { public S160(I159 s) { } }
    interface I161 { }
    class S161 : I161 { public S161(I160 s) { } }
    interface I162 { }
    class S162 : I162 { public S162(I161 s) { } }
    interface I163 { }
    class S163 : I163 { public S163(I162 s) { } }
    interface I164 { }
    class S164 : I164 { public S164(I163 s) { } }
    interface I165 { }
    class S165 : I165 { public S165(I164 s) { } }
    interface I166 { }
    class S166 : I166 { public S166(I165 s) { } }
    interface I167 { }
    class S167 : I167 { public S167(I166 s) { } }
    interface I168 { }
    class S168 : I168 { public S168(I167 s) { } }
    interface I169 { }
    class S169 : I169 { public S169(I168 s) { } }
    interface I170 { }
    class S170 : I170 { public S170(I169 s) { } }
    interface I171 { }
    class S171 : I171 { public S171(I170 s) { } }
    interface I172 { }
    class S172 : I172 { public S172(I171 s) { } }
    interface I173 { }
    class S173 : I173 { public S173(I172 s) { } }
    interface I174 { }
    class S174 : I174 { public S174(I173 s) { } }
    interface I175 { }
    class S175 : I175 { public S175(I174 s) { } }
    interface I176 { }
    class S176 : I176 { public S176(I175 s) { } }
    interface I177 { }
    class S177 : I177 { public S177(I176 s) { } }
    interface I178 { }
    class S178 : I178 { public S178(I177 s) { } }
    interface I179 { }
    class S179 : I179 { public S179(I178 s) { } }
    interface I180 { }
    class S180 : I180 { public S180(I179 s) { } }
    interface I181 { }
    class S181 : I181 { public S181(I180 s) { } }
    interface I182 { }
    class S182 : I182 { public S182(I181 s) { } }
    interface I183 { }
    class S183 : I183 { public S183(I182 s) { } }
    interface I184 { }
    class S184 : I184 { public S184(I183 s) { } }
    interface I185 { }
    class S185 : I185 { public S185(I184 s) { } }
    interface I186 { }
    class S186 : I186 { public S186(I185 s) { } }
    interface I187 { }
    class S187 : I187 { public S187(I186 s) { } }
    interface I188 { }
    class S188 : I188 { public S188(I187 s) { } }
    interface I189 { }
    class S189 : I189 { public S189(I188 s) { } }
    interface I190 { }
    class S190 : I190 { public S190(I189 s) { } }
    interface I191 { }
    class S191 : I191 { public S191(I190 s) { } }
    interface I192 { }
    class S192 : I192 { public S192(I191 s) { } }
    interface I193 { }
    class S193 : I193 { public S193(I192 s) { } }
    interface I194 { }
    class S194 : I194 { public S194(I193 s) { } }
    interface I195 { }
    class S195 : I195 { public S195(I194 s) { } }
    interface I196 { }
    class S196 : I196 { public S196(I195 s) { } }
    interface I197 { }
    class S197 : I197 { public S197(I196 s) { } }
    interface I198 { }
    class S198 : I198 { public S198(I197 s) { } }
    interface I199 { }
    class S199 : I199 { public S199(I198 s) { } }
    interface I200 { }
    class S200 : I200 { public S200(I199 s) { } }

    public static class CompilationTestDataProvider
    {
        public static void Register(IServiceCollection p)
        {
            p.AddTransient<I0, S0>();
            p.AddTransient<I0, S0>();
            p.AddTransient<I1, S1>();
            p.AddTransient<I2, S2>();
            p.AddTransient<I3, S3>();
            p.AddTransient<I4, S4>();
            p.AddTransient<I5, S5>();
            p.AddTransient<I6, S6>();
            p.AddTransient<I7, S7>();
            p.AddTransient<I8, S8>();
            p.AddTransient<I9, S9>();
            p.AddTransient<I10, S10>();
            p.AddTransient<I11, S11>();
            p.AddTransient<I12, S12>();
            p.AddTransient<I13, S13>();
            p.AddTransient<I14, S14>();
            p.AddTransient<I15, S15>();
            p.AddTransient<I16, S16>();
            p.AddTransient<I17, S17>();
            p.AddTransient<I18, S18>();
            p.AddTransient<I19, S19>();
            p.AddTransient<I20, S20>();
            p.AddTransient<I21, S21>();
            p.AddTransient<I22, S22>();
            p.AddTransient<I23, S23>();
            p.AddTransient<I24, S24>();
            p.AddTransient<I25, S25>();
            p.AddTransient<I26, S26>();
            p.AddTransient<I27, S27>();
            p.AddTransient<I28, S28>();
            p.AddTransient<I29, S29>();
            p.AddTransient<I30, S30>();
            p.AddTransient<I31, S31>();
            p.AddTransient<I32, S32>();
            p.AddTransient<I33, S33>();
            p.AddTransient<I34, S34>();
            p.AddTransient<I35, S35>();
            p.AddTransient<I36, S36>();
            p.AddTransient<I37, S37>();
            p.AddTransient<I38, S38>();
            p.AddTransient<I39, S39>();
            p.AddTransient<I40, S40>();
            p.AddTransient<I41, S41>();
            p.AddTransient<I42, S42>();
            p.AddTransient<I43, S43>();
            p.AddTransient<I44, S44>();
            p.AddTransient<I45, S45>();
            p.AddTransient<I46, S46>();
            p.AddTransient<I47, S47>();
            p.AddTransient<I48, S48>();
            p.AddTransient<I49, S49>();
            p.AddTransient<I50, S50>();
            p.AddTransient<I51, S51>();
            p.AddTransient<I52, S52>();
            p.AddTransient<I53, S53>();
            p.AddTransient<I54, S54>();
            p.AddTransient<I55, S55>();
            p.AddTransient<I56, S56>();
            p.AddTransient<I57, S57>();
            p.AddTransient<I58, S58>();
            p.AddTransient<I59, S59>();
            p.AddTransient<I60, S60>();
            p.AddTransient<I61, S61>();
            p.AddTransient<I62, S62>();
            p.AddScoped<I63, S63>();
            p.AddScoped<I64, S64>();
            p.AddScoped<I65, S65>();
            p.AddScoped<I66, S66>();
            p.AddScoped<I67, S67>();
            p.AddScoped<I68, S68>();
            p.AddScoped<I69, S69>();
            p.AddScoped<I70, S70>();
            p.AddScoped<I71, S71>();
            p.AddScoped<I72, S72>();
            p.AddScoped<I73, S73>();
            p.AddScoped<I74, S74>();
            p.AddScoped<I75, S75>();
            p.AddScoped<I76, S76>();
            p.AddScoped<I77, S77>();
            p.AddScoped<I78, S78>();
            p.AddScoped<I79, S79>();
            p.AddScoped<I80, S80>();
            p.AddScoped<I81, S81>();
            p.AddScoped<I82, S82>();
            p.AddScoped<I83, S83>();
            p.AddScoped<I84, S84>();
            p.AddScoped<I85, S85>();
            p.AddScoped<I86, S86>();
            p.AddScoped<I87, S87>();
            p.AddScoped<I88, S88>();
            p.AddScoped<I89, S89>();
            p.AddScoped<I90, S90>();
            p.AddScoped<I91, S91>();
            p.AddScoped<I92, S92>();
            p.AddScoped<I93, S93>();
            p.AddScoped<I94, S94>();
            p.AddScoped<I95, S95>();
            p.AddScoped<I96, S96>();
            p.AddScoped<I97, S97>();
            p.AddScoped<I98, S98>();
            p.AddScoped<I99, S99>();
            p.AddScoped<I100, S100>();
            p.AddScoped<I101, S101>();
            p.AddScoped<I102, S102>();
            p.AddScoped<I103, S103>();
            p.AddScoped<I104, S104>();
            p.AddScoped<I105, S105>();
            p.AddScoped<I106, S106>();
            p.AddScoped<I107, S107>();
            p.AddScoped<I108, S108>();
            p.AddScoped<I109, S109>();
            p.AddScoped<I110, S110>();
            p.AddScoped<I111, S111>();
            p.AddScoped<I112, S112>();
            p.AddScoped<I113, S113>();
            p.AddScoped<I114, S114>();
            p.AddScoped<I115, S115>();
            p.AddScoped<I116, S116>();
            p.AddScoped<I117, S117>();
            p.AddScoped<I118, S118>();
            p.AddScoped<I119, S119>();
            p.AddScoped<I120, S120>();
            p.AddScoped<I121, S121>();
            p.AddSingleton<I122, S122>();
            p.AddSingleton<I123, S123>();
            p.AddSingleton<I124, S124>();
            p.AddSingleton<I125, S125>();
            p.AddSingleton<I126, S126>();
            p.AddSingleton<I127, S127>();
            p.AddSingleton<I128, S128>();
            p.AddSingleton<I129, S129>();
            p.AddSingleton<I130, S130>();
            p.AddSingleton<I131, S131>();
            p.AddSingleton<I132, S132>();
            p.AddSingleton<I133, S133>();
            p.AddSingleton<I134, S134>();
            p.AddSingleton<I135, S135>();
            p.AddSingleton<I136, S136>();
            p.AddSingleton<I137, S137>();
            p.AddSingleton<I138, S138>();
            p.AddSingleton<I139, S139>();
            p.AddSingleton<I140, S140>();
            p.AddSingleton<I141, S141>();
            p.AddSingleton<I142, S142>();
            p.AddSingleton<I143, S143>();
            p.AddSingleton<I144, S144>();
            p.AddSingleton<I145, S145>();
            p.AddSingleton<I146, S146>();
            p.AddSingleton<I147, S147>();
            p.AddSingleton<I148, S148>();
            p.AddSingleton<I149, S149>();
            p.AddSingleton<I150, S150>();
            p.AddSingleton<I151, S151>();
            p.AddSingleton<I152, S152>();
            p.AddSingleton<I153, S153>();
            p.AddSingleton<I154, S154>();
            p.AddSingleton<I155, S155>();
            p.AddSingleton<I156, S156>();
            p.AddSingleton<I157, S157>();
            p.AddSingleton<I158, S158>();
            p.AddSingleton<I159, S159>();
            p.AddSingleton<I160, S160>();
            p.AddSingleton<I161, S161>();
            p.AddSingleton<I162, S162>();
            p.AddSingleton<I163, S163>();
            p.AddSingleton<I164, S164>();
            p.AddSingleton<I165, S165>();
            p.AddSingleton<I166, S166>();
            p.AddSingleton<I167, S167>();
            p.AddSingleton<I168, S168>();
            p.AddSingleton<I169, S169>();
            p.AddSingleton<I170, S170>();
            p.AddSingleton<I171, S171>();
            p.AddSingleton<I172, S172>();
            p.AddSingleton<I173, S173>();
            p.AddSingleton<I174, S174>();
            p.AddSingleton<I175, S175>();
            p.AddSingleton<I176, S176>();
            p.AddSingleton<I177, S177>();
            p.AddSingleton<I178, S178>();
            p.AddSingleton<I179, S179>();
            p.AddSingleton<I180, S180>();
            p.AddSingleton<I181, S181>();
            p.AddSingleton<I182, S182>();
            p.AddSingleton<I183, S183>();
            p.AddSingleton<I184, S184>();
            p.AddSingleton<I185, S185>();
            p.AddSingleton<I186, S186>();
            p.AddSingleton<I187, S187>();
            p.AddSingleton<I188, S188>();
            p.AddSingleton<I189, S189>();
            p.AddSingleton<I190, S190>();
            p.AddSingleton<I191, S191>();
            p.AddSingleton<I192, S192>();
            p.AddSingleton<I193, S193>();
            p.AddSingleton<I194, S194>();
            p.AddSingleton<I195, S195>();
            p.AddSingleton<I196, S196>();
            p.AddSingleton<I197, S197>();
            p.AddSingleton<I198, S198>();
            p.AddSingleton<I199, S199>();
            p.AddSingleton<I200, S200>();
        }
    }
}
