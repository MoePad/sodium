import javax.swing.*;
import java.awt.FlowLayout;
import swidgets.*;
import nz.sodium.*;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

public class dynamic_list {

    public static void main(String[] args) {
        JFrame frame = new JFrame("Dynamic List");
        frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        frame.setLayout(new FlowLayout());

        Transaction.runVoid(() -> {
          SButton addBtn = new SButton("Add");
          SButton rmBtn = new SButton("Remove");
          STextField input = new STextField("");

          CellLoop<List<String>> dynamicList = new CellLoop<>();

          Stream<String> sAdd = addBtn.sClicked.snapshot(input.text, (u, t) -> t);

          Stream<List<String>> sList = sAdd.snapshot(dynamicList,
                    (text, cellList) -> {
                      cellList.add(text);
                      return cellList;
                    });

          Cell<String> currentValue = dynamicList.map(list -> {
              String returnString = "";
              for(String s : list) {
                returnString += s + " ";
              }
              return returnString;
            });

          dynamicList.loop(sList.hold(new ArrayList<>()));

          SLabel lblSelection = new SLabel(currentValue);

          frame.add(rmBtn);
          frame.add(addBtn);
          frame.add(input);
          frame.add(lblSelection);
        });

        frame.setSize(400, 160);
        frame.setVisible(true);
    }

    static <A> Cell<List<A>> sequence(Collection<Cell<A>> in) {
        Cell<List<A>> out = new Cell<>(new ArrayList<A>());
        for (Cell<A> c : in)
            out = out.lift(c,
                (list0, a) -> {
                    List<A> list = new ArrayList<A>(list0);
                    list.add(a);
                    return list;
                });
        return out;
    }

    private static Integer parseInt(String t) {
        try {
            return Integer.parseInt(t);
        } catch (NumberFormatException e) {
            return 0;
        }
    }
}
