using System;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace MiCasaDB
{
    class Program
    {
        /// <summary>This method Logs in to the system.</summary>
        /// <param name="mail">the mail to connect with.</param>
        /// <param name="pass">the password to connect with in sha1.</param>
        /// <returns>A user id as int type. -1 if the details are wrong.</returns>
        static int Login(string mail, string pass, MySqlConnection con)
        {
            string stm = "SELECT u_id FROM users " +
                "where email=@email and pass=SHA1(@pass)";
            MySqlCommand cmd = new MySqlCommand(stm, con);
            cmd.Parameters.AddWithValue("@email", mail);
            cmd.Parameters.AddWithValue("@pass", pass);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                int u_id = rdr.GetInt32(0);
                rdr.Close();
                return u_id;
            }
            rdr.Close();
            return -1;
        }

        /// <summary>This method returns buildings of the user.</summary>
        /// <param name="u_id">the user id to check with.</param>
        /// <returns>A dynamic json with the information.</returns>
        static dynamic GetApartments(int u_id, MySqlConnection con)
        {
            string stm = "select a.b_id, a.a_id, b.country,b.city,b.street,b.number from apartments as a left outer join residence as r " +
                "on r.a_id = a.a_id LEFT OUTER JOIN buildings as b on a.b_id = b.b_id where r.u_id = @u_id";
            MySqlCommand cmd = new MySqlCommand(stm, con);
            cmd.Parameters.AddWithValue("@u_id", u_id);

            string jsonData = @"{data:[";
            MySqlDataReader rdr = cmd.ExecuteReader();

            int count = 0;
            while (rdr.Read())
            {
                if (count > 0)
                {
                    jsonData += ",";
                }
                jsonData += "{'b_id':" + rdr.GetInt32(0) + "," +
                    "'a_id':" + rdr.GetInt32(1) +
                    ",'country':'" + rdr.GetString(2) +
                    "','city':'" + rdr.GetString(3) +
                    "','street':'" + rdr.GetString(4) +
                    "','number':" + rdr.GetInt32(5) + "}";
                ++count;
            }
            jsonData += "]}";
            dynamic data = JObject.Parse(jsonData);
            rdr.Close();
            return data;
        }


        /// <summary>This method returns buildings of the user.</summary>
        /// <param name="u_id">the user id to check with.</param>
        /// <returns>A dynamic json with the information.</returns>
        static dynamic GetManagedBuildings(int u_id, MySqlConnection con)
        {
            string stm = "select m.b_id, m.is_admin,b.country,b.city,b.street,b.number from management as m " +
                "LEFT OUTER JOIN buildings as b on m.b_id = b.b_id where m.u_id = @u_id";
            MySqlCommand cmd = new MySqlCommand(stm, con);
            cmd.Parameters.AddWithValue("@u_id", u_id);

            string jsonData = @"{data:[";
            MySqlDataReader rdr = cmd.ExecuteReader();

            int count = 0;
            while (rdr.Read())
            {
                if (count > 0)
                {
                    jsonData += ",";
                }
                jsonData += "{'b_id':" + rdr.GetInt32(0) + "," +
                    "'is_admin':" + rdr.GetInt32(1) +
                    ",'country':'" + rdr.GetString(2) +
                    "','city':'" + rdr.GetString(3) +
                    "','street':'" + rdr.GetString(4) +
                    "','number':" + rdr.GetInt32(5) + "}";
                ++count;
            }
            jsonData += "]}";
            dynamic data = JObject.Parse(jsonData);
            rdr.Close();
            return data;
        }
        static dynamic GetApartmentsAndBuildings(int u_id, MySqlConnection cnn)
        {
            dynamic x = GetApartments(u_id, cnn);
            dynamic y = GetManagedBuildings(u_id, cnn);
            string jsonData = @"{u_id:" + u_id + "}";
            dynamic data = JObject.Parse(jsonData);

            JArray dataofJson0 = data.SelectToken("data");
            JArray dataOfJson1 = x.SelectToken("data");
            JArray dataofJson2 = y.SelectToken("data");
            foreach (JObject innerData in dataofJson2)
            {
                dataOfJson1.Add(innerData);
            }
            data["a_and_b"] = dataOfJson1;
            return data;
        }
        

        static void Main(string[] args)
        {
            string connetionString = null;
            MySqlDataReader rdr = null;
            MySqlConnection cnn;
            connetionString = "server=remotemysql.com;database=KHmZFwh4mA;uid=KHmZFwh4mA;pwd=hWP8GbQHs0;";
            cnn = new MySqlConnection(connetionString);
            try
            {
                cnn.Open();


                int u_id = Login("test5@test.com", "12345678", cnn);
                dynamic apartmentsAndBuildings = GetApartmentsAndBuildings(u_id, cnn);
                Console.WriteLine(apartmentsAndBuildings);


                cnn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        
    }
}
