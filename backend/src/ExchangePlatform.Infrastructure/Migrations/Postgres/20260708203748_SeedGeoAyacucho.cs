using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExchangePlatform.Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    // Seed geografico UBIGEO del departamento de Ayacucho (05): 11 provincias y
    // sus distritos (DatosGeograficos.md §5/§9). Idempotente por 'Ubigeo' unico.
    // Fuente: codificacion UBIGEO oficial (INEI). Los distritos mas recientes
    // deben verificarse contra la fuente antes de produccion (RGO-010).
    public partial class SeedGeoAyacucho : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Departamento
            migrationBuilder.Sql(@"
INSERT INTO ""Departamentos"" (""Ubigeo"", ""Nombre"")
SELECT '05', 'Ayacucho'
WHERE NOT EXISTS (SELECT 1 FROM ""Departamentos"" WHERE ""Ubigeo"" = '05');
");

            // 2. Provincias (11) — referencian al departamento por ubigeo.
            migrationBuilder.Sql(@"
INSERT INTO ""Provincias"" (""Ubigeo"", ""Nombre"", ""DepartamentoId"")
SELECT v.ubigeo, v.nombre, d.""Id""
FROM (VALUES
    ('0501', 'Huamanga'),
    ('0502', 'Cangallo'),
    ('0503', 'Huanca Sancos'),
    ('0504', 'Huanta'),
    ('0505', 'La Mar'),
    ('0506', 'Lucanas'),
    ('0507', 'Parinacochas'),
    ('0508', 'Páucar del Sara Sara'),
    ('0509', 'Sucre'),
    ('0510', 'Víctor Fajardo'),
    ('0511', 'Vilcas Huamán')
) AS v(ubigeo, nombre)
JOIN ""Departamentos"" d ON d.""Ubigeo"" = '05'
WHERE NOT EXISTS (SELECT 1 FROM ""Provincias"" p WHERE p.""Ubigeo"" = v.ubigeo);
");

            // 3. Distritos (119) — la provincia se deriva de los 4 primeros dígitos del ubigeo.
            migrationBuilder.Sql(@"
INSERT INTO ""Distritos"" (""Ubigeo"", ""Nombre"", ""ProvinciaId"")
SELECT v.ubigeo, v.nombre, p.""Id""
FROM (VALUES
    -- 0501 Huamanga
    ('050101', 'Ayacucho'), ('050102', 'Acocro'), ('050103', 'Acos Vinchos'),
    ('050104', 'Carmen Alto'), ('050105', 'Chiara'), ('050106', 'Ocros'),
    ('050107', 'Pacaycasa'), ('050108', 'Quinua'), ('050109', 'San José de Ticllas'),
    ('050110', 'San Juan Bautista'), ('050111', 'Santiago de Pischa'), ('050112', 'Socos'),
    ('050113', 'Tambillo'), ('050114', 'Vinchos'), ('050115', 'Jesús Nazareno'),
    ('050116', 'Andrés Avelino Cáceres Dorregaray'),
    -- 0502 Cangallo
    ('050201', 'Cangallo'), ('050202', 'Chuschi'), ('050203', 'Los Morochucos'),
    ('050204', 'María Parado de Bellido'), ('050205', 'Paras'), ('050206', 'Totos'),
    -- 0503 Huanca Sancos
    ('050301', 'Sancos'), ('050302', 'Carapo'), ('050303', 'Sacsamarca'),
    ('050304', 'Santiago de Lucanamarca'),
    -- 0504 Huanta
    ('050401', 'Huanta'), ('050402', 'Ayahuanco'), ('050403', 'Huamanguilla'),
    ('050404', 'Iguaín'), ('050405', 'Luricocha'), ('050406', 'Santillana'),
    ('050407', 'Sivia'), ('050408', 'Llochegua'), ('050409', 'Canayre'),
    ('050410', 'Uchuraccay'), ('050411', 'Pucacolpa'), ('050412', 'Chaca'),
    -- 0505 La Mar
    ('050501', 'San Miguel'), ('050502', 'Anco'), ('050503', 'Ayna'),
    ('050504', 'Chilcas'), ('050505', 'Chungui'), ('050506', 'Luis Carranza'),
    ('050507', 'Santa Rosa'), ('050508', 'Tambo'), ('050509', 'Samugari'),
    ('050510', 'Anchihuay'), ('050511', 'Oronccoy'),
    -- 0506 Lucanas
    ('050601', 'Puquio'), ('050602', 'Aucará'), ('050603', 'Cabana'),
    ('050604', 'Carmen Salcedo'), ('050605', 'Chaviña'), ('050606', 'Chipao'),
    ('050607', 'Huac-Huas'), ('050608', 'Laramate'), ('050609', 'Leoncio Prado'),
    ('050610', 'Llauta'), ('050611', 'Lucanas'), ('050612', 'Ocaña'),
    ('050613', 'Otoca'), ('050614', 'Saisa'), ('050615', 'San Cristóbal'),
    ('050616', 'San Juan'), ('050617', 'San Pedro'), ('050618', 'San Pedro de Palco'),
    ('050619', 'Sancos'), ('050620', 'Santa Ana de Huaycahuacho'), ('050621', 'Santa Lucía'),
    -- 0507 Parinacochas
    ('050701', 'Coracora'), ('050702', 'Chumpi'), ('050703', 'Coronel Castañeda'),
    ('050704', 'Pacapausa'), ('050705', 'Pullo'), ('050706', 'Puyusca'),
    ('050707', 'San Francisco de Ravacayco'), ('050708', 'Upahuacho'),
    -- 0508 Páucar del Sara Sara
    ('050801', 'Pausa'), ('050802', 'Colta'), ('050803', 'Corculla'),
    ('050804', 'Lampa'), ('050805', 'Marcabamba'), ('050806', 'Oyolo'),
    ('050807', 'Pararca'), ('050808', 'San Javier de Alpabamba'), ('050809', 'San José de Ushua'),
    ('050810', 'Sara Sara'),
    -- 0509 Sucre
    ('050901', 'Querobamba'), ('050902', 'Belén'), ('050903', 'Chalcos'),
    ('050904', 'Chilcayoc'), ('050905', 'Huacaña'), ('050906', 'Morcolla'),
    ('050907', 'Paico'), ('050908', 'San Pedro de Larcay'), ('050909', 'San Salvador de Quije'),
    ('050910', 'Santiago de Paucaray'), ('050911', 'Soras'),
    -- 0510 Víctor Fajardo
    ('051001', 'Huancapi'), ('051002', 'Alcamenca'), ('051003', 'Apongo'),
    ('051004', 'Asquipata'), ('051005', 'Canaria'), ('051006', 'Cayara'),
    ('051007', 'Colca'), ('051008', 'Huamanquiquia'), ('051009', 'Huancaraylla'),
    ('051010', 'Huaya'), ('051011', 'Sarhua'), ('051012', 'Vilcanchos'),
    -- 0511 Vilcas Huamán
    ('051101', 'Vilcas Huamán'), ('051102', 'Accomarca'), ('051103', 'Carhuanca'),
    ('051104', 'Concepción'), ('051105', 'Huambalpa'), ('051106', 'Independencia'),
    ('051107', 'Saurama'), ('051108', 'Vischongo')
) AS v(ubigeo, nombre)
JOIN ""Provincias"" p ON p.""Ubigeo"" = LEFT(v.ubigeo, 4)
WHERE NOT EXISTS (SELECT 1 FROM ""Distritos"" dd WHERE dd.""Ubigeo"" = v.ubigeo);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Distritos"" WHERE ""Ubigeo"" LIKE '05%';");
            migrationBuilder.Sql(@"DELETE FROM ""Provincias"" WHERE ""Ubigeo"" LIKE '05%';");
            migrationBuilder.Sql(@"DELETE FROM ""Departamentos"" WHERE ""Ubigeo"" = '05';");
        }
    }
}
