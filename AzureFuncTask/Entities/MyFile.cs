namespace AzureFuncTask.Entities
{
    public class MyFile
    {
        public MyFile()
        {

        }

        public MyFile(IFormFile file)
        {
            File = file;
        }

        public IFormFile File { get; set; }
    }
}
