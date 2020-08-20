using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMS_S_1._1
{
    class main_control
    {
        public void ams_controller()
        {
            
            Data_Base db_obj = new Data_Base();
            Logs logs_obj = new Logs(db_obj);
            
            string current_time_print = "NULL";
            if (db_obj.start_connection())
            {
               
                db_obj.get_machines();
                db_obj.connect_to_machines();
                while (true)
                {
                    DateTime NowTime = DateTime.Now;
                    //prepare time
                    string string_of_NowTime = (NowTime.Hour).ToString() + ":" + NowTime.Minute.ToString();//-12
                    
                    if(string_of_NowTime != current_time_print)
                    {
                        current_time_print = string_of_NowTime;
                        System.Console.WriteLine("Now Time -> {0} ", string_of_NowTime);
                    }
                       

                    if (string_of_NowTime == "6:5") //begin enrollment process
                    {
                        //begine enroll
                        db_obj.get_enroll_users();
                    }
                    else if(string_of_NowTime == "18:1")
                    {
                        //get logs
                        logs_obj.get_daily_logs();
                        //take att
                        logs_obj.take_att();
                    }
                }
               
                
            }
            else
            {
                Console.WriteLine("Database Not Connected");
            }
            

        }
    }
}
