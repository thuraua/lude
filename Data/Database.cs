using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using WpfAppCon02;

namespace Data
{
    public class Database
    {
        private SortedList<int, Car> collCars;
        private static Database db = null;
        private static OracleConnection conn = null;
        private SortedList<int, Owner> collOwners;
        private SortedList<int, Sale> collSales;
        private OracleTransaction trans = null;
        public System.Data.IsolationLevel IsolationLevel { get; set; } = System.Data.IsolationLevel.ReadCommitted;

        private Database(bool intern)
        {
            if (intern)
                conn = new OracleConnection(@"user id=d4b26;password=d4b;data source=" +
                                                     "(description=(address=(protocol=tcp)" +
                                                     "(host=192.168.128.152)(port=1521))(connect_data=" +
                                                     "(service_name=ora11g)))");

            else
                conn = new OracleConnection(@"user id=d4b26;password=d4b;data source=" +
                                                     "(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)" +
                                                     "(HOST=212.152.179.117)(PORT=1521))(CONNECT_DATA=" +
                                                     "(SERVICE_NAME=ora11g)))");
            conn.Open();
            collOwners = new SortedList<int, Owner>();
            collCars = new SortedList<int, Car>();
            collSales = new SortedList<int, Sale>();
        }

        public static Database NewInstance(bool intern)
        {
            return db == null ? new Database(intern) : db;
        }

        public void SwitchCarTransaction(Car car)
        {
            trans?.Commit();
            OracleCommand cmd1 = new OracleCommand("select * from cars where cnr = :cid for update nowait", conn);
            cmd1.Parameters.Add(new OracleParameter("cid", car.CarId));
            trans = conn.BeginTransaction(IsolationLevel);
            cmd1.Transaction = trans;
            cmd1.ExecuteNonQuery();
        }


        #region Reading Operations
        public IList<Sale> Read_Sales_from_DB(Owner o)
        {
            collSales.Clear();
            string select = "select * from sales where oid=:oid";
            OracleCommand cmd = new OracleCommand(select, conn);
            cmd.Parameters.Add(new OracleParameter("oid", o.ONr));
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Sale c = new Sale(Convert.ToInt32(reader["snr"]), Convert.ToInt32(reader["cid"]), Convert.ToInt32(reader["oid"]), Convert.ToDateTime(reader["saledate"]));
                    collSales.Add(c.SNR, c);
                }
            }
            return collSales.Values;
        }
        public IList<Car> Read_Cars_from_DB()
        {
            collCars.Clear();
            string select = "select * from cars where counter <>  0";
            OracleCommand cmd = new OracleCommand(select, conn);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Car c = new Car(Convert.ToInt32(reader["cnr"]), Convert.ToString(reader["cname"]), Convert.ToInt32(reader["counter"]));
                    collCars.Add(c.CarId, c);
                }
            }
            return collCars.Values;
        }
        public IList<Owner> Read_Owners_From_DB()
        {
            collOwners.Clear();
            string select = "select * from owner";
            OracleCommand cmd = new OracleCommand(select, conn);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Owner c = new Owner(Convert.ToInt32(reader["onr"]), Convert.ToString(reader["oname"]));
                    collOwners.Add(c.ONr, c);
                }
            }
            return collOwners.Values;
        }
        #endregion

        #region Sales Operations
        public void ExecuteSaleTransaction(Sale sale)
        {
            try
            {
                //trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                OracleCommand cmd2 = new OracleCommand("update cars set counter = counter - 1 where cnr = :cid", conn);
                cmd2.Parameters.Add(new OracleParameter("cid", sale.CID));
                OracleCommand cmd3 = new OracleCommand("insert into sales values(seq_sales.nextval, :cid, :oid, :saledate)", conn);
                cmd3.Parameters.Add(new OracleParameter("cid", sale.CID));
                cmd3.Parameters.Add(new OracleParameter("oid", sale.OID));
                cmd3.Parameters.Add(new OracleParameter("saledate", sale.SALEDATE));
                cmd2.Transaction = trans;
                cmd3.Transaction = trans;
                cmd2.ExecuteNonQuery();
                cmd3.ExecuteNonQuery();
                trans.Commit();
                trans = null;

            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
        }
        public void ExecuteResaleTransaction(Sale sale)
        {
            try
            {
                trans = conn.BeginTransaction(IsolationLevel);
                OracleCommand cmd2 = new OracleCommand("update cars set counter = counter + 1 where cnr = :cid", conn);
                cmd2.Parameters.Add(new OracleParameter("cid", sale.CID));
                OracleCommand cmd3 = new OracleCommand("delete from sales where snr=:id", conn);
                cmd3.Parameters.Add(new OracleParameter("id", sale.SNR));
                cmd2.Transaction = trans;
                cmd3.Transaction = trans;
                cmd2.ExecuteNonQuery();
                cmd3.ExecuteNonQuery();
                trans.Commit();
                trans = null;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
        }

        #endregion

        #region Car Operations

        /// <summary>
        /// Inserts the car into database's cars table.
        /// </summary>
        /// <param name="car">The car object to be added.</param>
        public void AddCar(Car car)
        {
            SwitchCarTransaction(car);
            OracleCommand cmd = new OracleCommand("insert into cars values(:id, :name, :bestand)", conn);
            cmd.Parameters.Add(new OracleParameter("id", car.CarId));
            cmd.Parameters.Add(new OracleParameter("name", car.CarName));
            cmd.Parameters.Add(new OracleParameter("bestand", car.Bestand));
            cmd.Transaction = trans;
            cmd.ExecuteNonQuery();
            trans.Commit();
            trans = null;
        }

        /// <summary>
        /// Updates all fields of the car in database.
        /// </summary>
        /// <param name="car">The car object to be updated.</param>
        public void UpdateCar(Car car)
        {
            SwitchCarTransaction(car);
            OracleCommand cmd = new OracleCommand("update cars set cname=:name, counter=:counter where cnr=:id", conn);
            cmd.Parameters.Add(new OracleParameter("name", car.CarName));
            cmd.Parameters.Add(new OracleParameter("counter", car.Bestand));
            cmd.Parameters.Add(new OracleParameter("id", car.CarId));
            cmd.Transaction = trans;
            cmd.ExecuteNonQuery();
            trans.Commit();
            trans = null;
        }

        /// <summary>
        /// Deletes the car and the sales involving it from database.
        /// </summary>
        /// <param name="selectedCar">The car object to be deleted.</param>
        public void RemoveCar(Car selectedCar)
        {
            SwitchCarTransaction(selectedCar);
            OracleCommand cmd = new OracleCommand("delete from cars where cnr=:id", conn);
            cmd.Parameters.Add(new OracleParameter("id", selectedCar.CarId));
            cmd.Transaction = trans;
            cmd.ExecuteNonQuery();
            trans.Commit();
            trans = null;
        }
        #endregion
    }
}
