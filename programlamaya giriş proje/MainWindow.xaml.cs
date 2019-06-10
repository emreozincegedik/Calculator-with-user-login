using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;


namespace programlamaya_giriş_proje
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {   //for extra security
        /*public static String sha256_hash(string value) 
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }*/

        public MainWindow()
        {
            InitializeComponent();
            tologin();            
        }

        string username, password, userlog, userfile, userfromfile, passwordfromfile;
        bool n;
        private string usersfilename = "users";
        private string savefilelocation= "D:\\Karışık\\ders\\"; //change this for different computer

        private void navigate_click(object sender, MouseButtonEventArgs e)
        {
            if (n) 
            {
                if (register.IsFocused)
                {
                    login_username.Text = "";
                    register.IsEnabled = false;
                }
                else
                {
                    calculator.IsEnabled = false;
                    btn_clear_Click(sender, e);
                    Logs.Items.Clear();                    
                }
                login_passwd.Password = register_username.Text = register_passwd.Password = register_passwdagain.Password = "";
                tologin();
            }
            else
                toregister();
        }
       
        public void tologin()
        {
            Window.Height = Window.Width = 350;
            login.IsEnabled = true;
            login.Focus();
            Navigate.Text = "Not a member? Sign Up!";
            n = false;
        }

        public void toregister()
        {
            register.IsEnabled = true;
            register.Focus();
            Navigate.Text = "Already a member? Sign in here!";
            n = true;
            login_username.Text = "";
        }

        public void tocalc()
        {
            calculator.IsEnabled = true;
            calculator.Focus();
            Window.Width = 500;
            Window.Height = 485;            
            Navigate.Text = "Log out";
            LoggedUser.Text = "Logged in as: "+username;
            n = true;
        }

        private void userlogfile(string user)
        {
            userlog = savefilelocation+ user+".txt";
        } 

        private void usersfile()
        {
            userfile = savefilelocation + usersfilename +".txt";
        }

        private void userfileandpassword(string a)
        {
            userfromfile = a.Substring(0, a.IndexOf(':'));
            passwordfromfile = a.Substring(a.IndexOf(':') + 1);
        }


        private void btn_login_Click(object sender, RoutedEventArgs e)
        {            
            login_username.Text = login_username.Text.Trim();
            username = login_username.Text.ToLower();
            password = login_passwd.Password;
            if (username.Length == 0||password.Length==0)
            {
                MessageBox.Show("Username or password is empty");
                return;
            }            
            if (username.Any(ch => !char.IsNumber(ch) && !char.IsLetter(ch)))
            {
                MessageBox.Show("Username can't contain special characters and white space");
                return;
            }

            usersfile();
            System.IO.StreamReader file = new System.IO.StreamReader(@userfile);
            string line;
            while ((line=file.ReadLine())!=null) //doesn't accept file.Readline()!=null correctly for some reason
            {
                userfileandpassword(line);
                if (userfromfile == username && passwordfromfile != password)
                {
                    MessageBox.Show("Password is wrong");
                    return;
                }
                if (userfromfile == username && passwordfromfile == password)
                {
                    tocalc();

                    userlogfile(username);
                    System.IO.StreamReader log = new System.IO.StreamReader(@userlog);
                    string line2;
                    while ((line2=log.ReadLine()) != null)                    {
                        Logs.Items.Insert(0, line2);                    }
                    log.Close();
                    file.Close();
                    return;
                }

            }
            MessageBox.Show("There is no registered username");
            file.Close();
            return;

        }

        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            register_username.Text = register_username.Text.Trim();
            username = register_username.Text.ToLower();
            password = register_passwd.Password;
            usersfile();
            System.IO.StreamReader file = new System.IO.StreamReader(@userfile);
            string line;
            while ((line=file.ReadLine()) != null)
            {
                userfileandpassword(line);
                if (userfromfile == username)
                {
                    MessageBox.Show("Username already exists");
                    file.Close();
                    return;
                }
            }
            file.Close();

            if (username.Length == 0||password.Length==0)
            {
                MessageBox.Show("Username or password is empty");
                return;
            }
            if (username.Any(ch => !char.IsNumber(ch) && !char.IsLetter(ch)))
            {
                MessageBox.Show("Username can't contain special characters and white space");
                return;
            }
            if (username== usersfilename) //I couldn't get a better solution for this
            {
                MessageBox.Show("This would create a userlog named users.txt \r\n That file contains registered users \r\n I can't allow that username");
                return;
            }
            if (password.Length<8)
            {
                MessageBox.Show("Password has to be at least 8 characters");
                return;
            }
            if (password != register_passwdagain.Password)
            {
                MessageBox.Show("Please re-enter password correctly");
                return;
            }            
            if (!password.Any(char.IsUpper)|| !password.Any(char.IsLower)|| !password.Any(char.IsNumber))
            {
                MessageBox.Show("Password must contain at least 1 uppercase 1 lowercase and 1 number");
                return;
            }

            using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(userfile, true))
                {
                    file2.WriteLine(username + ":" + password);
                    file2.Close();
                }
            userlogfile(username);
            System.IO.StreamWriter log = new System.IO.StreamWriter(userlog);
            log.Close();
            tocalc();
            register_username.Text = register_passwd.Password = register_passwdagain.Password = "";
        }

        string operation;
        int paranteses = 0;
        bool paranteses_opened = false;
        bool paranteses_closed = false;
        bool comma = false;
        bool equalclicked = false;

        private void btn_equal_Click(object sender, RoutedEventArgs e)
        {
            int operatornum = 0;
            string s = calculator_equation.Text;
            foreach (char c in s)
            {
                if (c == '+'|| c == '-'|| c == '*'|| c == '/') operatornum++;                
            }
            if (operatornum > 0 && !(operatornum == 1 && (s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/")))) //if there is an operator and (it doesn't end with operator if only one operator exists) => it goes inside the code
            {
                if (s.EndsWith(".") || s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/"))
                {
                    s = s.Substring(0, s.Length - 1);
                    calculator_equation.Text = s;
                }
                while (paranteses > 0)
                {
                    btn_close_paranteses(sender, e);
                }
                s = calculator_equation.Text;
                DataTable dt = new DataTable();
                calculator_answer.Text = String.Format("{0:n}", dt.Compute(s, "")).ToString();

                userlogfile(username);
                string equationwithanswer = s + "=" + calculator_answer.Text;
                using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(userlog, true))
                {
                    file2.WriteLine(equationwithanswer);
                    file2.Close();
                }
                Logs.Items.Insert(0, equationwithanswer);

                calculator_equation.Text = calculator_answer.Text;
                equalclicked = true;
            }
        }

        private void btn_operation_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string s = calculator_equation.Text;
            operation = (btn.Content).ToString();
            
            if (s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/") || s.EndsWith("."))
                s = s.Substring(0, s.Length - 1);            
            if ((s.EndsWith("(") || s == ""))
            {
                if ((operation == "+" || operation == "-"))
                {
                    s += "0"+operation;
                    goto out1; //to prevent (/ or (* from happening
                }
            }
            else
            {
                s += operation;
            }
            out1:
            comma = paranteses_opened = paranteses_closed = equalclicked = false;
            calculator_equation.Text = s;
        }

        private void btn_number_Click(object sender, RoutedEventArgs e)
        {
            
            Button btn = (Button)sender;
            string s = calculator_equation.Text;
            if (equalclicked)
            {
                s = "";
            }

            if ((btn.Content).ToString() == ".")
            {
                if (comma == false)
                {
                    if (s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/") || s.EndsWith("(")|| s == "")
                            s += "0.";
                    else if (s.EndsWith(")"))
                        s += "*0.";
                    else
                        s += ".";
                    comma = true;
                }
            }
            else
            {
                if (s.EndsWith(")"))
                    s += "*" + btn.Content;
                else
                    s += btn.Content;
            }
            paranteses_opened = paranteses_closed = equalclicked = false;
            calculator_equation.Text = s;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            calculator_equation.Text = "";
            calculator_answer.Text = "0.00";
            comma = paranteses_opened = paranteses_closed = equalclicked = false;
            paranteses = 0;
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            string s = calculator_equation.Text;

            if (s.EndsWith("("))
            {
                paranteses_opened = false;
                paranteses--;
            }
            else if (s.EndsWith(")"))
            {
                paranteses_closed = false;
                paranteses++;
            }
            else if (s.EndsWith("."))
            {
                comma = false;
            }

            if (s.Length > 1)
                s = s.Substring(0, s.Length - 1);
            else
                s = "";

            if (s.EndsWith("(")) //in case the last digit isnt number
            {
                paranteses_opened = true;
                paranteses_closed = false;
            }
            else if (s.EndsWith(")"))
            {
                paranteses_closed = true;
                paranteses_opened = false;
            }
            else if (s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/"))
            {
                paranteses_opened = false;
                paranteses_closed = false;
            }
            else if (s.EndsWith("."))
            {
                comma = true;
            }
            equalclicked = false;
            calculator_equation.Text = s;
        }

        private void btn_open_paranteses(object sender, RoutedEventArgs e)
        {
            string s = calculator_equation.Text;
            paranteses++;
            if (s.EndsWith("."))
            {
                s = s.Substring(0, s.Length - 1);
            }
            if (!s.EndsWith("+") && !s.EndsWith("-") && !s.EndsWith("*") && !s.EndsWith("/"))
            {
                if (paranteses_opened == false)
                {
                    s += "*(";
                    if (s == "*(")   //this is for making *( to (, otherwise it would stay ...*(                 
                        s = "(";
                }
                else
                    s += "(";
            }
            else
                s += "(";
            paranteses_opened = true;
            paranteses_closed = equalclicked = false;
            calculator_equation.Text = s;
        }

        private void btn_close_paranteses(object sender, RoutedEventArgs e)
        {
            string s = calculator_equation.Text;
            if (paranteses > 0)
            {
                if (s.EndsWith(".") || s.EndsWith("+") || s.EndsWith("-") || s.EndsWith("*") || s.EndsWith("/"))
                    s = s.Substring(0, s.Length - 1);
                if (paranteses_opened == true)
                    s += "0)";
                else
                    s += ")";

                paranteses--;                
                paranteses_closed = true;
                paranteses_opened = equalclicked = false;
                calculator_equation.Text = s;
            }
        }

        private void Btn_clearlogs_Click(object sender, RoutedEventArgs e)
        {
            userlogfile(username);
            System.IO.StreamWriter log = new System.IO.StreamWriter(userlog);
            log.Close();
            Logs.Items.Clear();
        }

        private void key(Button name)
        {
            name.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void calculator_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.Key.ToString()); //I used this to see the key names            
            switch (e.Key.ToString())
            {
                case "D1":
                    key(btn_1);
                    break;
                case "D2":
                    key(btn_2);
                    break;
                case "D3":
                    key(btn_3);
                    break;
                case "D4":
                    if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                        key(btn_plus);
                    else
                        key(btn_4);
                    break;
                case "D5":
                    key(btn_5);
                    break;
                case "D6":
                    key(btn_6);
                    break;
                case "D7":
                    key(btn_7);
                    break;
                case "D8":
                    if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                        key(btn_bracket1);
                    else
                        key(btn_8);
                    break;
                case "D9":
                    if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                        key(btn_bracket2);
                    else
                        key(btn_9);
                    break;
                case "D0":
                    key(btn_0);
                    break;
                case "OemPeriod":
                    key(btn_dot);
                    break;
                case "OemMinus":
                    key(btn_minus);
                    break;
                case "Divide":
                    key(btn_divide);
                    break;
                case "Multiply":
                    key(btn_multiply);
                    break;
                case "Subtract":
                    key(btn_minus);
                    break;
                case "Add":
                    key(btn_plus);
                    break;
                case "Return":
                    key(btn_equal);
                    break;
                case "Back":
                    key(btn_delete);
                    break;
                case "Decimal":
                    key(btn_dot);
                    break;
                case "NumPad1":
                    key(btn_1);
                    break;
                case "NumPad2":
                    key(btn_2);
                    break;
                case "NumPad3":
                    key(btn_3);
                    break;
                case "NumPad4":
                    key(btn_4);
                    break;
                case "NumPad5":
                    key(btn_5);
                    break;
                case "NumPad6":
                    key(btn_6);
                    break;
                case "NumPad7":
                    key(btn_7);
                    break;
                case "NumPad8":
                    key(btn_8);
                    break;
                case "NumPad9":
                    key(btn_9);
                    break;
                case "NumPad0":
                    key(btn_0);
                    break;
                default:
                    break;
            }
        }
    }
}