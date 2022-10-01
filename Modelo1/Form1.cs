using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Globalization;

namespace Parcial2
{
    public partial class Form1 : Form
    {

        // objetos privados para compartir en este formulario
        private DataSet ds;
        private OleDbDataAdapter daR; //para la tabla Repuestos
        private OleDbDataAdapter daM; //para la tabla Marcas
        private const string PATH_ARCHIVO = "ReporteRepuesto.txt";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //conexion a la DB

            OleDbConnection cnn = new OleDbConnection();
            cnn.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + Application.StartupPath + "\\AutoRep.mdb";
            cnn.Open();

            //configuracion para la tabla Repuestos

            OleDbCommand cmR = new OleDbCommand();
            cmR.Connection = cnn;
            cmR.CommandType = CommandType.TableDirect;
            cmR.CommandText = "Repuestos";

            daR = new OleDbDataAdapter(cmR);

            ds = new DataSet();

            daR.Fill(ds, "Repuestos");

            OleDbCommandBuilder cbR = new OleDbCommandBuilder(daR);


            //Crear comando para usar la tabla marcas

            OleDbCommand cmM = new OleDbCommand();
            cmM.Connection = cnn;
            cmM.CommandType = CommandType.TableDirect;
            cmM.CommandText = "Marcas";

            daM = new OleDbDataAdapter(cmM);

            daM.Fill(ds, "Marcas");

            OleDbCommandBuilder cbM = new OleDbCommandBuilder(daM);

            //definimos la clave primaria para la tabla Marcas

            DataColumn[] dcM = new DataColumn[1];
            dcM[0] = ds.Tables["Marcas"].Columns["IdMarca"];
            ds.Tables["Marcas"].PrimaryKey = dcM;

            cmbMarca.DisplayMember = "Nombre";
            cmbMarca.ValueMember = "IdMarca";
            cmbMarca.DataSource = ds.Tables["Marcas"];




            // para cargar los campos vacios

            txtDescripcion.Text = "";
            txtNumero.Text = "";
            txtPrecio.Text = "";

        }


        //metodo para validar los campos no esten vacios y para validar que el numero de repuesto no se repita
        private bool validar()
        {
            bool resultado = false;

            if(txtNumero.Text != "" && txtPrecio.Text != "" && txtDescripcion.Text != "" && cmbOrigen.SelectedIndex != -1)
            {

                

                if(ds.Tables["Repuestos"].Rows.Count < 1)
                {
                    resultado = true;
                }
                else
                {
                    foreach (DataRow dr in ds.Tables["Repuestos"].Rows)
                    {
                        if ((int)dr["Numero"] == int.Parse(txtNumero.Text))
                        {
                            MessageBox.Show("El numero de repuesto ya existe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            resultado = true;
                        }
                    }
                }
            }

            return resultado;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if(validar() == true)
            {
                //configuramos para agregar un nuevo registro

                DataRow dr = ds.Tables["Repuestos"].NewRow();

                dr["Numero"] = txtNumero.Text;
                dr["Descripcion"] = txtDescripcion.Text;
                dr["Precio"] = int.Parse(txtPrecio.Text);
                dr["IdMarca"] = cmbMarca.SelectedIndex;
                dr["Origen"] = cmbOrigen.GetItemText(cmbOrigen.SelectedItem);

                //agregamos el nuevo registro a la tabla

                ds.Tables["Repuestos"].Rows.Add(dr);

                //actualizamos la tabla en la DB

                daR.Update(ds, "Repuestos");

                //generacion de reporte y grabado en archivo txt

                if (PATH_ARCHIVO != "")
                {
                    // crea el stream en modo append (para agregar sin borrar el contenido del archivo)
                    StreamWriter sw = new StreamWriter(PATH_ARCHIVO, true);
                    // graba la linea con los valores de los campos
                    sw.WriteLine(txtNumero.Text.ToString() + "," + txtDescripcion.Text + "," +
                        decimal.Parse(txtPrecio.Text).ToString("#.00", CultureInfo.InvariantCulture) + "," +
                        cmbOrigen.GetItemText(cmbOrigen.SelectedItem)  + ","+  cmbMarca.GetItemText(cmbMarca.SelectedItem));
                    sw.Close();  // cerrar el stream
                    sw.Dispose(); // liberar los recursos
                    
                }



                txtDescripcion.Text = "";
                txtNumero.Text = "";
                txtPrecio.Text = "";
            }
            else{
                MessageBox.Show("Datos incorrectos o Faltanes", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //para mostrar los datos en el dataGridView
        private void btnConsultar_Click(object sender, EventArgs e)
        {

            dgvRepuestos.Rows.Clear();

            decimal total = 0;

            if (ds.Tables["Repuestos"].Rows.Count > 0)
            {
                //foreach para recorrer la tabla y tomar los valores que necesitamos

                foreach(DataRow dr in ds.Tables["Repuestos"].Rows)
                {
                    dgvRepuestos.Rows.Add(dr["Numero"], dr["Descripcion"], dr["Precio"]);

                    total += decimal.Parse(dr["Precio"].ToString());
                }
            }

            txtTotal.Text = total.ToString();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
