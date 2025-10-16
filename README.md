# Unity Dropdown UI
This project was originally made for work but decided to make it as an external Extension. Custom dropdown that supports parent with childs option data and search function

## Installation
### UnityPackage
You can download the unity package from the release page

### UPM Package
You can download via upm `https://github.com/herghys/Unity-Custom-Dropdown.git`

## Usage
1. Put the Dropdown into canvas from prefab. Prefab located in Samples
2. Modify the settings if needed

![image](https://github.com/user-attachments/assets/9859941c-3250-41c5-9b71-d28a67016af4)

3. You need to Initialize it first by calling from another class / manager
    ```cs
    Initialize(Dictionary<string, IEnumerable<object>> values)
    ```
    or
    ```cs
    Initialize(Dictionary<string, string[]> values)
    ```
4. Filters will appear when you select the Dropdown / typing.
## Dependencies
This package required `TextMeshPro Package`. I'm not implementing Legacy Text.
